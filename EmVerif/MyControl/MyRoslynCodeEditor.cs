using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using RoslynPad.Editor;

namespace EmVerif.MyControl
{
    class MyRoslynCodeEditor : RoslynCodeEditor
    {
        public MyRoslynCodeEditor()
        {
            TextChanged += MyTextChanged;
        }

        public MyString MyText
        {
            get
            {
                return (MyString)GetValue(MyTextProperty);
            }
            set
            {
                SetValue(MyTextProperty, value);
            }
        }

        public static readonly DependencyProperty MyTextProperty =
            DependencyProperty.Register(
                "MyText",
                typeof(MyString),
                typeof(MyRoslynCodeEditor),
                new PropertyMetadata(default(MyString), OnMyTextPropertyChanged));

        private static void OnMyTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != null)
            {
                ((MyString)e.OldValue).Content = ((MyRoslynCodeEditor)d).Text;
            }
            ((MyRoslynCodeEditor)d).Text = ((MyString)e.NewValue).Content;
        }

        private static void MyTextChanged(object sender, EventArgs e)
        {
            ((MyRoslynCodeEditor)sender).MyText.Content = ((MyRoslynCodeEditor)sender).Text;
        }
    }
}
