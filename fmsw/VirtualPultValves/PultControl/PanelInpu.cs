using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;

namespace VirtualPultValves.PultControl
{
    public class PanelInpu : Panel
    {
        public int TypePanel { get; set; }
        public PanelInpu()
        {
            TypePanel = 1;
        }
        protected override Size MeasureOverride(Size sizeAvailable)
        {

              double proporcion = 1.3333333333333333333333333333333;
              if (TypePanel == 2) proporcion = 1.6;
           
                Size sizeDesired = new Size(0, 0);
                double xW = 0d;
                double xH = 0d;
              
                if (sizeAvailable.Height * proporcion < sizeAvailable.Width)
                {
                    xW = sizeAvailable.Height * proporcion;
                    xH = sizeAvailable.Height;
                }
                else
                {
                    xH = sizeAvailable.Width / proporcion;
                    xW = sizeAvailable.Width;
                }

                foreach (UIElement child in InternalChildren)
                {
                    if (xW < 10000)
                    {
                        child.SetValue(WidthProperty, xW);
                        child.SetValue(HeightProperty, xH);
                    }
                    child.Measure(sizeAvailable);
                    sizeDesired.Width += child.DesiredSize.Width;
                    sizeDesired.Height += child.DesiredSize.Height;

                }

                return sizeDesired;
          

           



        }

        protected override Size ArrangeOverride(Size sizeFinal)
        {

            for (int i = 0; i < InternalChildren.Count; i++)
            {
                var child = InternalChildren[i];
                Rect rect = new Rect();
                if (TypePanel == 1)
                {
                    rect = new Rect(
                        new Point((sizeFinal.Width - child.DesiredSize.Width),
                                  (sizeFinal.Height - child.DesiredSize.Height)),
                                  child.DesiredSize);
                }
                if (TypePanel == 2)
                {
                    rect = new Rect(
                       new Point((sizeFinal.Width / 2 - child.DesiredSize.Width / 2),
                                 (sizeFinal.Height / 2 - child.DesiredSize.Height / 2)),
                                 child.DesiredSize);
                } 

                child.Arrange(rect);
            }
            return sizeFinal;
        }
    }
}
