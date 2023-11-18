﻿using prog_p1.Core;
using prog_p1.MVM.View_Mode_;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace prog_p1.MVM.View_Mode_
{
     class MainViewModel :ObservableObject
    {

        public RelayCommand HomeViewCommand { get; set; }
        public RelayCommand LeaderViewCommand { get; set; }
        public RelayCommand ExitCommand { get; }




        public HomeViewModel HomeVM { get; set; }
        public LeaderBoardViewModel LeaderVm { get; set; }

        private object _curentView;

        public object CurentView
        {
            get { return _curentView; }
            set { _curentView = value;
                OnPropertyChange();
            }
        }

        public MainViewModel()
        {
            HomeVM = new HomeViewModel();
            LeaderVm = new LeaderBoardViewModel();

            CurentView = HomeVM;

            HomeViewCommand = new RelayCommand(o =>
            {
                CurentView= HomeVM;
            });

            LeaderViewCommand = new RelayCommand(o =>
            {
                CurentView = LeaderVm;
            });
            ExitCommand = new RelayCommand(o =>
            {
                // Perform any necessary cleanup or other actions before exiting
                Environment.Exit(0); // Exit the application
            });


        }
    }
}