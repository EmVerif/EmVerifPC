using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace EmVerif.MyControl
{
    class MyTextBox : TextBox
    {
        public MyTextBox()
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
                typeof(MyTextBox),
                new PropertyMetadata(default(MyString), OnMyTextPropertyChanged));

        private static void OnMyTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != null)
            {
                ((MyString)e.OldValue).Content = ((MyTextBox)d).Text;
            }
            ((MyTextBox)d).Text = ((MyString)e.NewValue).Content;
        }

        private static void MyTextChanged(object sender, EventArgs e)
        {
            ((MyTextBox)sender).MyText.Content = ((MyTextBox)sender).Text;
        }
    }
}
