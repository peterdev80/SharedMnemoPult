#include <Windows.h>

#include "hostinpu.h"

using namespace System;
using namespace System::Windows::Interop;
using namespace System::Runtime::InteropServices;
using namespace fmslapi;
using namespace System::Diagnostics;
using namespace System::Collections::Generic;
using namespace System::Threading;
using namespace System::Windows::Controls;
using namespace System::Windows::Input;

namespace AVIAKOM
{
	public ref class InpuPresenter : public ContentControl
	{
	private:
		UIElement^ _failed;

		ControlHost ^_hst;
		void OnInpuLoaded();
		void OnInpuFailed(Exception ^exception);

	public:
		InpuPresenter(double height, double width, int InpuNum, UIElement ^Loading, UIElement ^Failed);
		void PressNeptKey(INT32 Num, INT32 Key);

		property bool HideCursor
		{
			void set(bool);
		}

	protected:
		virtual void OnPreviewKeyDown(KeyEventArgs^) override;
	};
}