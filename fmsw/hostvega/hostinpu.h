#include <Windows.h>

using namespace System;
using namespace System::Windows;
using namespace System::Windows::Interop;
using namespace System::Runtime::InteropServices;
using namespace fmslapi;
using namespace System::Diagnostics;
using namespace System::Collections::Generic;
using namespace System::Threading;

typedef void (__stdcall *pInpu_Start)(HWND hPanel1, HWND hPanel2, INT32 PowerState1, INT32 PowerState2, void* InpuParam);
typedef void (__stdcall *pInpu_Run)();
typedef void (__stdcall *pInpu_Stop)(INT32 InpuParam);
typedef void (__stdcall *pInpu_Resize)(INT32 Width, INT32 Height);
typedef void (__stdcall *pInpu_PressNeptKey)(INT32 InpuNo, INT32 NeptKey);

typedef void (__cdecl *pUserTransport)(void *Buffer, UINT32 &Len, UINT32 Sender, UINT32 Receiver);
typedef void (__cdecl *pVegaUserTransport)(void *Buffer, UINT32 &Len);
typedef void (__stdcall *pINPU_netInit)(pUserTransport Snd, pUserTransport *Rcv);
        
typedef void (__stdcall *pSV_netInit)(pVegaUserTransport Snd, pVegaUserTransport *Rcv);
typedef void (__stdcall *pSV_setPath)(char *Path);

typedef array<Byte>^ barr;

namespace AVIAKOM
{
	private ref class ControlHost  : public HwndHost
	{
	private:
		System::Windows::Forms::Form ^_hf;
		System::Windows::Forms::Timer ^_hft;
		HWND hwndInpu;
		HMODULE _hlib;

		void OnpResize(Object ^s, EventArgs ^e);
		void OnpTick(Object ^s, EventArgs ^e);

		pInpu_Run Inpu_Run;
		pInpu_Resize Inpu_Resize;
		pInpu_Stop Inpu_Stop;
		pInpu_PressNeptKey Inpu_PressNeptKey;
		pVegaUserTransport SV_receive;
		pUserTransport Inpu_Receive;

        int _inpuType;
        int hostHeight, hostWidth;
        IChannel ^_ioneptun;
        IChannel ^_iointerinpu;

        Queue<Int32> ^_nk;
		bool _failed;

		ManualResetEvent ^_unloaded;

		void Worker(Object ^state);

		//void VegaReceived(ISenderChannel ^Sender, array<Byte>^ Data);
		void NeptunSend(void *Buffer, UINT32 %Len, UINT32 Sender, UINT32 Receiver);

		[UnmanagedFunctionPointer(CallingConvention::Cdecl)]
		delegate void INPUUserTransport(void *Buffer, UInt32 %Len, UInt32 Sender, UInt32 Receiver);

	internal:
		void BeginInit() new;

	public:
		ControlHost(double height, double width, int InpuNum);

		void PressNeptKey(INT32 Num, INT32 Key);

		Action^ InpuLoaded;
		Action<Exception^>^ Failed;

		bool HideCursor;

	protected:
		virtual HandleRef BuildWindowCore(HandleRef hwndParent) override;
		virtual void DestroyWindowCore(HandleRef hwnd) override;
	};
}

#pragma managed(push, off)
#pragma pack(1)
struct InpuPacket
{
	UINT32 Magic;
	UINT16 Sender;
	UINT16 Receiver;
	UINT32 ID;
	UINT32 NumPack;
};
#pragma managed(pop)