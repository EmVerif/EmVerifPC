﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using EmVerif.Core.Script;
using EmVerif.Model;

namespace EmVerif.MainWindowViewModel
{
    class ViewModel : INotifyPropertyChanged
    {
        public IEnumerable<IPAddress> IpAddressList { get; private set; }
        public SaveButton SaveButtonInstance { get; private set; }
        public LoadButton LoadButtonInstance { get; private set; }
        public event PropertyChangedEventHandler PropertyChanged;

        public IPAddress SelectedIpAddress
        {
            get
            {
                return Database.Instance.SelectedIpAddress;
            }
            set
            {
                Database.Instance.SelectedIpAddress = value;
            }
        }

        public ViewModel()
        {
            IpAddressList = PublicController.Instance.GetIpV4List();
            SaveButtonInstance = new SaveButton();
            LoadButtonInstance = new LoadButton();
            Database.Instance.PropertyChanged += Database_PropertyChanged;
        }

        private void Database_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Database.Instance.SelectedIpAddress):
                    OnPropertyChanged(nameof(SelectedIpAddress));
                    break;
            }
        }

        private void OnPropertyChanged(string info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
    }
}
