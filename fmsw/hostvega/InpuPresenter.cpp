#include "InpuPresenter.h"

namespace AVIAKOM
{
	InpuPresenter::InpuPresenter(double height, double width, int InpuNum, UIElement ^Loading, UIElement ^Failed)
	{
		_failed = Failed;
		Content = Loading;
		_hst = gcnew ControlHost(height, width, InpuNum);
		_hst->InpuLoaded += gcnew Action(this, &InpuPresenter::OnInpuLoaded);
		_hst->Failed += gcnew Action<Exception^>(this, &InpuPresenter::OnInpuFailed);

		_hst->BeginInit();
		HideCursor = false;
	}

	void InpuPresenter::OnInpuLoaded()
	{
		Content = _hst;
	}

	void InpuPresenter::OnInpuFailed(Exception ^ex)
	{
		Content = _failed;

		if(_failed == nullptr)
			return;
		
		auto T = _failed->GetType();
		auto mi = T->GetMethod("AssignException");
		if (mi == nullptr)
			return;

		mi->Invoke(_failed, gcnew array<Object^>(1) {ex} );
	}

	void InpuPresenter::PressNeptKey(INT32 Num, INT32 Key)
	{
		_hst->PressNeptKey(Num, Key);
	}

	void InpuPresenter::OnPreviewKeyDown(KeyEventArgs ^e)
	{
	}

	void InpuPresenter::HideCursor::set(bool value)
	{
		_hst->HideCursor = value;
	}
}
