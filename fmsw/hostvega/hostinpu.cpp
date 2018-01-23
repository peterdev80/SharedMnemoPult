// Размещение ИнПУ в WPF форме

#include <tchar.h>
#include "hostinpu.h"
#include "hpet.h"

#pragma managed(push, off)
void LoadNeptunFont();
void UnloadNeptunFont();
#pragma managed(pop)

namespace AVIAKOM
{
	ControlHost::ControlHost(double height, double width, int InpuNum)
	{
		HideCursor = false;

		_io_q = gcnew Queue<Tuple<Byte, array<Byte>^>^>();
		//_vs_q = gcnew Queue<array<Byte>^>();
		_nk = gcnew Queue<Int32>();

		_inpuType = InpuNum;
		hostHeight = (int)height;
		hostWidth = (int)width;
	}

	void ControlHost::BeginInit()
	{
		ThreadPool::QueueUserWorkItem(gcnew WaitCallback(this, &ControlHost::Worker));
	}

	void ControlHost::Worker(Object ^state)
	{
		#if _DEBUG
			Thread::CurrentThread->Name = "InPU Hosting";
		#endif

		#pragma region Связные функции
		pInpu_Start Inpu_Start;
		pINPU_netInit Inpu_netInit;
		pSV_setPath SV_setResPath;
		pSV_setPath SV_setConfigPath;
		pSV_netInit SV_netInit;
#pragma endregion

		SetErrorMode(SEM_FAILCRITICALERRORS);
		//SetDllDirectory(_T(".\\vega\\"));						// В ./vegabfi/ ресурсы и конфиг. Код загружается из ./vega/
		_hlib = LoadLibrary(_T("InPU.dll"));

		auto man = APIHost::GetAssociatedAPIHost("iwks")->Manager;

		if (_hlib == NULL) 
		{
			LPTSTR str;

			FormatMessage(FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_FROM_SYSTEM, NULL, 
						  GetLastError(),
						  MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT),
						  (LPTSTR)&str,
						  128, NULL);

			Dispatcher->BeginInvoke(Failed, gcnew array<Exception^>(1) { 
				gcnew ExternalException("Компонент пульта (InPU.dll) не может быть загружен.\r\n" + Marshal::PtrToStringAuto(IntPtr(str)), 0xdeadbeef) });

			LocalFree(str);

			return;
		}

		#pragma region Получение указателей на связные функции
		Inpu_Start = (pInpu_Start)GetProcAddress(_hlib, "Inpu_Start");
		Inpu_Run = (pInpu_Run)GetProcAddress(_hlib, "Inpu_Run");
		Inpu_Stop = (pInpu_Stop)GetProcAddress(_hlib, "Inpu_Stop");
		Inpu_Resize = (pInpu_Resize)GetProcAddress(_hlib, "Inpu_Resize");
		Inpu_PressNeptKey = (pInpu_PressNeptKey)GetProcAddress(_hlib, "Inpu_PressNeptKey");
		Inpu_netInit = (pINPU_netInit)GetProcAddress(_hlib, "INPU_netInit");
		SV_netInit = (pSV_netInit)GetProcAddress(_hlib, "SV_netInit");
		SV_setResPath = (pSV_setPath)GetProcAddress(_hlib, "SV_setResPath");
		SV_setConfigPath = (pSV_setPath)GetProcAddress(_hlib, "SV_setConfigPath");

		if (Inpu_Start == nullptr ||
			Inpu_Run == nullptr ||
			Inpu_Stop == nullptr ||
			Inpu_Resize == nullptr ||
			Inpu_PressNeptKey == nullptr ||
			Inpu_netInit == nullptr ||
			SV_netInit == nullptr ||
			SV_setResPath == nullptr ||
			SV_setConfigPath == nullptr)
		{
			auto str = String::Format("Inpu_Start=0x{0}\r\n", IntPtr(Inpu_Start).ToString("X"));
			str += String::Format("Inpu_Run=0x{0}\r\n", IntPtr(Inpu_Run).ToString("X"));
			str += String::Format("Inpu_Stop=0x{0}\r\n", IntPtr(Inpu_Stop).ToString("X"));
			str += String::Format("Inpu_Resize=0x{0}\r\n", IntPtr(Inpu_Resize).ToString("X"));
			str += String::Format("Inpu_PressNeptKey=0x{0}\r\n", IntPtr(Inpu_PressNeptKey).ToString("X"));
			str += String::Format("Inpu_netInit=0x{0}\r\n", IntPtr(Inpu_netInit).ToString("X"));
			str += String::Format("SV_netInit=0x{0}\r\n", IntPtr(SV_netInit).ToString("X"));
			str += String::Format("SV_setResPath=0x{0}\r\n", IntPtr(SV_setResPath).ToString("X"));
			str += String::Format("SV_setConfigPath=0x{0}\r\n", IntPtr(SV_setConfigPath).ToString("X"));

			Dispatcher->BeginInvoke(Failed, gcnew array<Exception^>(1) { gcnew ExternalException("Неверный набор экспортируемых InPU.dll функций.\r\n-----\r\n" + str, 0xdeadbeef) });
			return;
		}

#pragma endregion

		auto dlg = gcnew INPUUserTransport(this, &ControlHost::NeptunSend);
		auto gch = GCHandle::Alloc(dlg);
		auto dlgptr = (pUserTransport)Marshal::GetFunctionPointerForDelegate(dlg).ToPointer();

		LoadNeptunFont();

		pVegaUserTransport sr;
		pUserTransport ir;

		Inpu_netInit(dlgptr, &ir); Inpu_Receive = ir;			// Инициализация сетевого обмена ИнПУ
		SV_netInit(NULL, &sr); SV_receive = sr;					// Инициализация сетевого обмена БФИ в составе ИнПУ
		//SV_setResPath(".\\vega\\");								// Инициализация СКГИ БФИ в составе ИнПУ
		//SV_setConfigPath("sc_config_bfi.xml");

		if (Inpu_Receive == nullptr ||
			SV_receive == nullptr)
		{
			auto str = String::Format("Inpu_Receive=0x{0}\r\n", IntPtr(Inpu_Receive).ToString("X"));
			str += String::Format("SV_receive=0x{0}\r\n", IntPtr(SV_receive).ToString("X"));

			Dispatcher->BeginInvoke(Failed, gcnew array<Exception^>(1) { gcnew ExternalException("Не удалось инициализировать обмен данными.\r\n-----\r\n" + str, 0xdeadbeef) });
			return;
		}

		_ctl = man->JoinVariablesChannel("Control", "FMSControl", nullptr, nullptr);
		_iwd = _ctl->GetWatchDogVariable(String::Format("__FMS_WD_INPU{0}_LOADED", _inpuType));

		if (_iwd->Value)
		{
			Dispatcher->BeginInvoke(Failed, gcnew array<Exception^>(1) { gcnew ExternalException("Нельзя запустить несколько экземпляров ИнПУ.", 0xdeadbeef) });
			return;	
		}

		_hf = gcnew System::Windows::Forms::Form();
		hwndInpu = (HWND)_hf->Handle.ToPointer();
		_hf->Width = 800;
		_hf->Height = 600;
		_hf->Visible = false;

		_hf->Resize += gcnew EventHandler(this, &ControlHost::OnpResize);

		_hft = gcnew System::Windows::Forms::Timer();
		_hft->Interval = 10;
		_hft->Tick += gcnew EventHandler(this, &ControlHost::OnpTick);
		_hft->Enabled = true;

		_iwd->CheckDups = false;
		_iwd->AutoSend = true;
		_iwd->Reset(60);

		switch(_inpuType)
		{
		case 1: Inpu_Start(hwndInpu, NULL, 2, 2, NULL); break;
		case 2: Inpu_Start(NULL, hwndInpu, 2, 2, NULL); break;
		}

		if (HideCursor)
			ShowCursor(false);

		// Канал данных от модели
		_ioneptun = man->JoinChannel("IO_NEPTUN", gcnew DataReceived(this, &ControlHost::NeptunReceived)); 
		_ioneptun->UsePacketOrder = gcnew Reorder::PacketReorder();

		// Канал данных в модель
		_ioneptuntomodel = man->JoinChannel("IO_NEPTUN_TO_MODEL", nullptr);

		// Канал обмена ИнПУ между собой
		_iointerinpu = man->JoinChannel("IO_INTERINPU", gcnew DataReceived(this, &ControlHost::InterInpuReceived));  
		_iointerinpu->UsePacketOrder = gcnew Reorder::PacketReorder();

		// Канал данных визуализации БФИ
		//_iovs = man->JoinChannel("IO_VS", gcnew DataReceived(this, &ControlHost::VegaReceived));                     

		auto vl = _ctl->GetKVariable("__FMS_VEGA_LOADED_ACK");
		vl->AutoSend = true;
		vl->Set();

		auto _fps = _ctl->GetFloatVariable(String::Format("__FMS_INPU{0}_FPS", _inpuType));
		_fps->AutoSend = true;
		_fps->CheckDups = true;

		auto fq = gcnew Queue<int>();
		for(int i=0; i<200; i++)
			fq->Enqueue(0);

		Dispatcher->BeginInvoke(InpuLoaded);

		//HANDLE ht = CreateEvent(NULL, FALSE, FALSE, NULL);
		//hpet::AddEvent(ht);

		System::Windows::Forms::Application::Run();

		_unloaded->Set();

		_ioneptun->Leave();
		_ioneptuntomodel->Leave();
		_iointerinpu->Leave();
		//_iovs->Leave();

		UnloadNeptunFont();

		gch.Free();
	}

	void ControlHost::OnpResize(Object ^Sender, EventArgs ^e)
	{
		Inpu_Resize(_hf->Width, _hf->Height);
	}

	void ControlHost::OnpTick(Object ^Sender, EventArgs ^e)
	{
		//VSEPCommandBuffer ipack;

		static int phase = 0;

		if(Inpu_Run != NULL)
		{
			if (++phase == 40)
			{
				phase = 0;
				_iwd->Reset();
			}

			// Нажатие клавиш пульта
			Monitor::Enter(_nk);
			while(_nk->Count > 0)
			{
				try
				{
					Inpu_PressNeptKey(_inpuType, _nk->Dequeue());
				}
				catch(SEHException ^ex)
				{
					_failed = true;
					Dispatcher->BeginInvoke(Failed, gcnew array<Exception^>(1) { ex });
					break;
				}
			}	
			Monitor::Exit(_nk);

			// Данные ИнПУ
			Monitor::Enter(_io_q);
			while(_io_q->Count > 0)
			{
				auto d = _io_q->Dequeue();
				pin_ptr<Byte> dp = &d->Item2[0];
				UINT32 len = d->Item2->Length;

				Inpu_Receive(dp, len, d->Item1, _inpuType);
			}
			Monitor::Exit(_io_q);

			// Данные СКГИ
			/*Monitor::Enter(_vs_q);
			while(_vs_q->Count > 0)
			{
				auto d = _vs_q->Dequeue();
				pin_ptr<Byte> dp = &d[0];
				UINT32 len = d->Length;
				SV_receive(dp, len);
			}
			Monitor::Exit(_vs_q);*/

			/*CreatePacket(ipack);
			UINT32 l = ipack.GetLength();

			if (l != 0 && !_failed)
				SV_receive(ipack.GetBuffer(), l);*/

			Inpu_Run();
		}

		if (_unloaded == nullptr)
			return;

		Inpu_Run = NULL;
		_hft->Enabled = false;

		Inpu_Stop(0);
		FreeLibrary(_hlib);

		System::Windows::Forms::Application::Exit();
	}

	// Размещение окна ИнПУ на форме WPF
	HandleRef ControlHost::BuildWindowCore(HandleRef hwndParent)
	{
		SetParent(hwndInpu, (HWND)hwndParent.Handle.ToPointer());
		SetWindowLongPtr(hwndInpu, GWL_STYLE, WS_CHILD | WS_VISIBLE);

		SetFocus((HWND)hwndParent.Handle.ToPointer());

		return HandleRef(this, IntPtr(hwndInpu));
	}
		
	// Удаление размещенного ранее окна с формы WPF
	void ControlHost::DestroyWindowCore(HandleRef hwnd)
	{
		SetWindowLongPtr(hwndInpu, GWL_STYLE, 0);
		SetParent(hwndInpu, NULL);

		_unloaded = gcnew ManualResetEvent(FALSE);
		_unloaded->WaitOne();
		_unloaded = nullptr;
	}

	// Прием данных от соседнего ИнПУ
	void ControlHost::InterInpuReceived(ISenderChannel ^Sender, ReceivedMessage ^Message)
	{
		auto Data = Message->Data;

		if (_failed)
			return;

		auto sender = _inpuType == 1 ? 2 : 1;

		Monitor::Enter(_io_q);
		_io_q->Enqueue(gcnew Tuple<Byte, barr>(sender, Data));
		Monitor::Exit(_io_q);
	}

	// Прием данных от модели
	void ControlHost::NeptunReceived(ISenderChannel ^Sender, ReceivedMessage ^Message)
	{
		auto Data = Message->Data;

		if (_failed)
			return;

		Monitor::Enter(_io_q);
		_io_q->Enqueue(gcnew Tuple<Byte, barr>(3, Data));
		Monitor::Exit(_io_q);
	}

	// Прием данных от модели для СКГИ
	/*void ControlHost::VegaReceived(ISenderChannel ^Sender, array<Byte>^ Data)
	{
		if (_failed)
			return;

		Monitor::Enter(_vs_q);
		_vs_q->Enqueue(Data);
		Monitor::Exit(_vs_q);
	}*/

	// Передача данных в модель и соседнее ИнПУ
	void ControlHost::NeptunSend(void *Buffer, UINT32 %Len, UINT32 Sender, UINT32 Receiver)
	{
		if (_failed)
			return;

		// Отправка в модель
		if (Receiver == 3 && _ioneptuntomodel != nullptr)
			_ioneptuntomodel->SendMessage(IntPtr(Buffer), (int)Len);

		// Отправка на соседний ИнПУ
		if (Receiver == (_inpuType == 1 ? 2 : 1) && _iointerinpu != nullptr)
			_iointerinpu->SendMessage(IntPtr(Buffer), (int)Len);
	}

	// Нажатие клавиш на пульте
	void ControlHost::PressNeptKey(INT32 Num, INT32 Key)
	{
		if (_failed)
			return;

		Monitor::Enter(_nk);
		_nk->Enqueue(Key);
		Monitor::Exit(_nk);
	}
}

