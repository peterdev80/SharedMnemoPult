using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Data;
using VirtualPultValves.DataAccess;

namespace VirtualPultValves.Model
{
  public  class TransporantModel: DependencyObject, IDisposable
    {
      private string _tn = String.Empty;
      private Brush _PassiveColor;
      private int _ts = 12;

        #region Public Property
      public int TextSize
      {
          get { return _ts; }
          set
          {
              if (value != _ts)
                  _ts = value;
          }

      }
      /// <summary>
      /// Надпись на транспоранте
      /// </summary>
      public string TransporantName
      {
          get { return ((string)_tn).Replace("^", Environment.NewLine); }
          set
          {
              if (value != _tn)
              {
                  _tn = value;
              }
          }
      }
      /// <summary>
      /// Цвет активного транспоранта
      /// </summary>
      public Brush ActiveColor { get; set; }

      /// <summary>
      /// Цвет пасивного транспоранта
      /// </summary>
      public Brush PassiveColor
      {
          get { return _PassiveColor; }
          set
          {
              if (value == _PassiveColor) return;
              _PassiveColor = value;
              if (!ValTC) TCColor = PassiveColor;
          }
      }


      private string _varL;
     /// <summary>
     /// Биндинг для модели вне тс
     /// </summary>
      public string VarVal
      {
          get { return _varL; }
          set
          {
              if (_varL == value) return;
              _varL = value;
              TCModelBinding(_varL);
          }
      }

        #endregion
        #region DependecyProperty
      /// <summary>
      /// Значение переменной true=Activ
      /// </summary>
      public static readonly DependencyProperty ValTCProperty = DependencyProperty.Register(
        "ValTC", typeof(bool), typeof(TransporantModel), new FrameworkPropertyMetadata(false,
    FrameworkPropertyMetadataOptions.AffectsRender, OnValTCChange));
      public bool ValTC
      {
          get { return (bool)GetValue(ValTCProperty); }
          set { this.SetValue(ValTCProperty, value); }
      }

      private static void OnValTCChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
      {
          TransporantModel con = d as TransporantModel;
          if ((bool)e.NewValue)
              con.TCColor = con.ActiveColor;
          else
              con.TCColor = con.PassiveColor;




      }


      public static readonly DependencyProperty TCColorProperty =
          DependencyProperty.Register("TCColor", typeof(Brush), typeof(TransporantModel));

      public Brush TCColor
      {
          get { return (Brush)GetValue(TCColorProperty); }
          set { this.SetValue(TCColorProperty, value); }
      }

      /*public static readonly DependencyProperty TCTextSizeProperty =
         DependencyProperty.Register("TextSize", typeof(int), typeof(TransporantModel),new FrameworkPropertyMetadata(12,
    FrameworkPropertyMetadataOptions.AffectsRender));
      public int TextSize
      {
          get { return (int)GetValue(TCTextSizeProperty); }
          set { this.SetValue(TCTextSizeProperty, value); }
      }*/
        
        
        
        #endregion
 
      
      
      public void TCModelBinding(string varBind)
      {
          /*
          // Так надо, чтобы не привязываться к comlib.dll явно
          try
          {
              if (Trigger == null)
                  Trigger = Activator.CreateInstance("comlib", "comlib.UpdateTriggers.WagoUpdateTrigger").Unwrap() as ut.TriggerBase;
          }
          catch (SystemException) { }

          VariablesHost vh = VarHostMy.GetVarHostMy;
          var BindValue = vh.GetBoolVariable(varBind);
          BindValue.UpdateTrigger = Trigger;

          BindingOperations.SetBinding(this, TransporantModel.ValTCProperty, new Binding { Source = BindValue, Path = new PropertyPath(Variable.ValueProperty) });
          */

      }
      #region IDisposable Members

      /// <summary>
      /// Invoked when this object is being removed from the application
      /// and will be subject to garbage collection.
      /// </summary>
      public void Dispose()
      {
          this.OnDispose();
      }

      /// <summary>
      /// Child classes can override this method to perform 
      /// clean-up logic, such as removing event handlers.
      /// </summary>
      protected virtual void OnDispose()
      {
          BindingOperations.ClearAllBindings(this);

      }
      #endregion
    

    }
}
