// Размещение ИнПУ в WPF форме

#include <tchar.h>
#include "hostinpu.h"
#include "hpet.h"

#include <stdio.h>
using namespace System::IO; 
using namespace System::Text;


#pragma managed(push, off)
void LoadNeptunFont();
void UnloadNeptunFont();
#pragma managed(pop)

namespace AVIAKOM
{

	/*struct TFormReq
	{
		UINT32 magic;
		UINT16 sender;
		UINT16 receiver;
		UINT32 id;
		UINT32 num;
		UINT8 extId;
		int formNum;
		UINT16 sf1;
		UINT16 sf2;
	};*/

	ControlHost::ControlHost(double height, double width, int InpuNum)
	{
		HideCursor = false;

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

		switch(_inpuType)
		{
		case 1: Inpu_Start(hwndInpu, NULL, 2, 2, NULL); break;
		case 2: Inpu_Start(NULL, hwndInpu, 2, 2, NULL); break;
		}

		if (HideCursor)
			ShowCursor(false);

		// Канал данных модели
		_ioneptun = gcnew Channel(_inpuType == 1 ? "Inpu1Port" : "Inpu2Port", "ModelAddr");

		// Канал обмена ИнПУ между собой
		_iointerinpu = gcnew Channel(nullptr, _inpuType == 1 ? "Inpu2Addr" : "Inpu1Addr");

		// Канал данных визуализации БФИ
		//_iovs = man->JoinChannel("IO_VS", gcnew DataReceived(this, &ControlHost::VegaReceived));                     

		Dispatcher->BeginInvoke(InpuLoaded);

		//HANDLE ht = CreateEvent(NULL, FALSE, FALSE, NULL);
		//hpet::AddEvent(ht);

		System::Windows::Forms::Application::Run();

		_unloaded->Set();

		_ioneptun->Leave();
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
		if(Inpu_Run != NULL)
		{
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
			array<Byte> ^d;
			while ((d = _ioneptun->TryGetMessage()) != nullptr)
			{
				pin_ptr<Byte> dp = &d[0];
				UINT32 len = d->Length;

				auto rd = gcnew System::IO::BinaryReader(gcnew System::IO::MemoryStream(d));

				if (rd->ReadUInt32() != 0x71AF5A13)
					continue;

				auto sender = rd->ReadUInt16();
				auto receiver = rd->ReadUInt16();
				auto id = rd->ReadInt32();
				auto num = rd->ReadUInt32();
				auto extid = rd->ReadByte();


				if (receiver != 0 && receiver != 2 && receiver != 1)
					continue;

				if (id == 2 && extid == 3)
				{
					auto n = rd->ReadInt32();

					if (n != 0)
					{
						int a = 2;
					}
				}
				Inpu_Receive(dp, len, sender, receiver == 0 ? _inpuType : receiver);
			}
#pragma region Trash



		/*	while((d = _ioneptun->TryGetMessage()) != nullptr)
			{
				pin_ptr<Byte> dp = &d[0];
				UINT32 len = d->Length;

				auto rd = gcnew System::IO::BinaryReader(gcnew System::IO::MemoryStream(d));

				if (rd->ReadUInt32() != 0x71AF5A13)
					continue;

				auto sender = rd->ReadUInt16();
				auto receiver = rd->ReadUInt16();							
				auto id = rd->ReadUInt32();
				auto num = rd->ReadUInt32();
				
			
				TFormReq fr;
				fr.magic = 0x71AF5A13;
				fr.sender = sender;
				fr.receiver = receiver;
				fr.id = id;
				fr.num = num;
				
				
				//if (id == 0x0002)
				{
					auto extId = rd->ReadByte();
					auto formNum = rd->ReadInt32();
					fr.sf1 = rd->ReadUInt16();
					fr.sf2 = rd->ReadUInt16();
					fr.extId = extId;
				fr.formNum = 0;

					//if (extId == 0x03)
					//{

						

						

						UINT32 len1 = sizeof(TFormReq);

						

						

				

						//sw->WriteLine("+++++++++++++++++++++");
						//for (int i = 0; i < d->Length; i++)
						//{
						//	/*if (i == 0) sw->WriteLine("Magic Val");
						//	if (i == 4) sw->WriteLine("Sender");
						//	if (i == 6) sw->WriteLine("Reciver");
						//	if (i == 8) sw->WriteLine("Id");
						//	if (i == 12) sw->WriteLine("NUM");
						//	if (i == 16) sw->WriteLine("ExtId");
						//	if (i == 17) sw->WriteLine("Data");
						//	sw->WriteLine(dp[i]);
						//	sw->WriteLine(sender);
						//	sw->WriteLine(receiver);
						//	sw->WriteLine(id);
						//	sw->WriteLine(num);
						//	sw->WriteLine(extId);
						//	sw->WriteLine(formNum);
						//	sw->WriteLine(fr.formNum);
						//	sw->WriteLine("----------------------------------------");						
						//}
						//sw->Close();
					//}
				//}
						
							
				//Inpu_Receive(dp, len, sender, receiver);  
				//Inpu_Receive(&fr, len, sender, _inpuType);
				FILE *fp = fopen("packet.txt", "a+");
				if (fp)
				{
					fprintf(fp, "num = %d", num);
					fclose(fp);
				}
		//	if(!sended) Inpu_Receive(dp, len, sender, _inpuType);
		//	}
		*/
#pragma endregion
			
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

	// Передача данных в модель и соседнее ИнПУ
	void ControlHost::NeptunSend(void *Buffer, UINT32 %Len, UINT32 Sender, UINT32 Receiver)
	{
		if (_failed)
			return;

		// Отправка в модель
		if (Receiver == 3 && _ioneptun != nullptr)
			_ioneptun->SendMessage(IntPtr(Buffer), (int)Len);

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

