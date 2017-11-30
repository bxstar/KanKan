using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using System.Windows.Input;
using ExtendPropertyLib.WPF;
using ExtendPropertyLib;
using LOLVideo.Models;
using LOLVideo.Videos.LOL178;
using LOLVideo.Helper;
using System.Windows;
using LOLVideo.Views;
using System.IO;
using System.Diagnostics;
namespace LOLVideo
{
    [Export(typeof(IShell))]
    public class MainViewModel : ViewModelBase<MainInfo>, IShell
    {
        public override void OnDoCreate(ExtendObject item, params object[] args)
        {
            base.OnDoCreate(item, args);
            ServiceManager.AddService<IWindowManager>(new WindowManager());


            OnSelectedItemCommand = RegisterCommand(OnSelectedItem);
            OnOpenViewsCheckedCommand = RegisterCommand(OnOpenViewsChecked);
            OnHeroSelectedCommand = RegisterCommand(OnHeroSelected);
            OnAlbumSelectedCommand = RegisterCommand(OnAlbumSelected);
            OnApplyTPCommand = RegisterCommand(OnApplyTP);

            popupView = CreateView<SelectVideoPlayerDialogViewModel>(true);
            DownLoadManagerView = CreateView<DownloadManagerViewModel>(true);
            ModifySkinDialogView = CreateView<ModifySkinDialogViewModel>(true);

            DownloadManager.Info = DownLoadManagerView.Model;
            //CurrentAlbumProperty.ValueChanged += CurrentAlbumProperty_ValueChanged;

            AlbumList = new ObservableCollection<Album>{
                new Wow178Album("http://wow.178.com/list/pvpshipin/index.html"){Name = "pvp视频区"},
                new Wow178Album("http://wow.178.com/list/quweishipin/index.html"){Name="娱乐视频区"},
                new Wow178Album("http://wow.178.com/list/ctmsp/index.html"){Name="大灾变视频区"},
                new Wow178Album("http://wow.178.com/list/49616960521.html"){Name="我叫MT系列"},
                new Wow178Album("http://wow.178.com/list/lk/index.html"){Name="巫妖王视频区"},
                new Wow178Album("http://wow.178.com/list/rs/index.html"){Name="晶红圣殿视频"},
                new Wow178Album("http://wow.178.com/list/ghxc/index.html"){Name="公会宣传视频"},
                new Wow178Album("http://wow.178.com/list/cg/index.html"){Name="官方视频"},
                new Wow178Album("http://wow.178.com/list/fanchang/index.html"){Name = "玩家翻唱区"},
            };

        }

        public DownloadManagerViewModel DownLoadManagerView { set; get; }

        public ModifySkinDialogViewModel ModifySkinDialogView { set; get; }

        private SelectVideoPlayerDialogViewModel popupView;

        public override string GetViewTitle()
        {
            return "WOW Viewer - 魔兽世界视频观看聚集地";
        }

        public static ExtendProperty HeroListProperty = RegisterProperty<MainViewModel>(v => v.HeroList);
        public ObservableCollection<Hero> HeroList { set { SetValue(HeroListProperty, value); } get { return (ObservableCollection<Hero>)GetValue(HeroListProperty); } }


        public static ExtendProperty AlbumListProperty = RegisterProperty<MainViewModel>(v => v.AlbumList);
        /// <summary>
        /// 绑定的专辑列表
        /// </summary>
        public ObservableCollection<Album> AlbumList { set { SetValue(AlbumListProperty, value); } get { return (ObservableCollection<Album>)GetValue(AlbumListProperty); } }


        public static ExtendProperty CurrentAlbumProperty = RegisterProperty<MainViewModel>(v => v.CurrentAlbum);
        public Album CurrentAlbum { set { SetValue(CurrentAlbumProperty, value); } get { return (Album)GetValue(CurrentAlbumProperty); } }

        public static ExtendProperty IsLoadingProperty = RegisterProperty<MainViewModel>(v => v.IsLoading);
        [Sync]
        public bool IsLoading { set { SetValue(IsLoadingProperty, value); } get { return (bool)GetValue(IsLoadingProperty); } }


        public static ExtendProperty IsOpenViewProperty = RegisterProperty<MainViewModel>(v => v.IsOpenView);
        /// <summary>
        /// 无限视距
        /// </summary>
        public bool IsOpenView { set { SetValue(IsOpenViewProperty, value); } get { return (bool)GetValue(IsOpenViewProperty); } }

        public static ExtendProperty IsNoTPProperty = RegisterProperty<MainViewModel>(v => v.IsNoTP);
        /// <summary>
        /// 是否是TP文件
        /// </summary>
        public bool IsNoTP { set { SetValue(IsNoTPProperty, value); } get { return (bool)GetValue(IsNoTPProperty); } }



        public ICommand OnSelectedItemCommand { set; get; }

        public ICommand OnOpenViewsCheckedCommand { set; get; }

        public ICommand OnHeroSelectedCommand { set; get; }

        public ICommand OnAlbumSelectedCommand { set; get; }

        public ICommand OnApplyTPCommand { set; get; }



        public override async void OnLoad()
        {
            //string path = ShareSetting.Setting.LOLPath = OperateIniFile.ReadIniData("Game", "RootPath", "", ShareSetting.ConfigPath);
            ShareSetting.Setting.ImagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
            ShareSetting.Setting.DownloadsPath = OperateIniFile.ReadIniData("Game", "Downloads", "", ShareSetting.ConfigPath);
            if (string.IsNullOrEmpty(ShareSetting.Setting.DownloadsPath))
                ShareSetting.Setting.DownloadsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Downloads");

            //if (string.IsNullOrEmpty(path))
            //{
            //    var dialog = new FolderBrowserDialog();
            //    dialog.Description = "运行本程序前，先设置LOL目录";
            //    if (dialog.ShowDialog() == DialogResult.OK)
            //    {
            //        path = dialog.SelectedPath;
            //        ShareSetting.Setting.LOLPath = path;
            //        OperateIniFile.WriteIniData("Game", "RootPath", path, ShareSetting.ConfigPath);
            //    }
            //}

            //IsOpenView = Model.CheckView();
            //IsNoTP = Model.CheckNoTP();

            //LoadHeros();

            IsOpenView = true;
            IsNoTP = false;

            CurrentAlbum = AlbumList[0];
            LoadVideos();
        }

        private async void LoadHeros()
        {
            var imagepath = ShareSetting.Setting.ImagePath;
            HeroList = new ObservableCollection<Hero>(await Hero.GetHeros());
            foreach (var hero in HeroList)
                await hero.Load(imagepath);
        }


        public async void LoadVideos()
        {
            try
            {
                IsLoading = true;
                var imagePath = ShareSetting.Setting.ImagePath;
                await CurrentAlbum.Next();
                var po = new ParallelOptions() { MaxDegreeOfParallelism = 2 };
                Parallel.ForEach(CurrentAlbum.Videos, po,
                                 v => v.LoadImage(imagePath));
            }
            catch { }
            IsLoading = false;
        }

        public async void OnSelectedItem(object item)
        {
            if (item != null)
            {
                this.BuildUp(item);
                var videoInfo = item as Video;
                popupView.PageIndex = -1;
                popupView.PageNumbers = null;
                popupView.Model.Video = videoInfo;

                var mv = this.View as IMainView;
                mv.ShowPopup(popupView, videoInfo.Name, false, false, false, true);
            }
        }

        public void OnOpenViewsChecked()
        {
            if (!Model.TurnOFFViews(!IsOpenView))
            {
                IsOpenView = !IsOpenView;
            }


        }

        public void OnApplyTP()
        {
            try
            {
                Model.TurnOFFNoTP(IsNoTP);
            }
            catch (Exception e)
            {
                var wm = ServiceManager.GetService<IWindowManager>();
                wm.ShowMessage(e.Message);

            }

        }






        public async void OnAlbumSelected(object item)
        {
            if (item != null)
            {
                var mv = this.View as IMainView;
                mv.HeroListNoSelected();

                mv.ScrollTop();
                CurrentAlbum = item as Album;
                if (CurrentAlbum.Videos.Count <= 0)
                {
                    LoadVideos();
                }

            }
        }


        public async void OnHeroSelected(object item)
        {
            if (item != null)
            {
                var mv = this.View as IMainView;
                mv.AlbumListNoSelected();
                mv.ScrollTop();
                var hero = item as Hero;
                CurrentAlbum = hero.HeroAlbum;
                if (CurrentAlbum.Videos.Count <= 0)
                {
                    LoadVideos();
                }

            }
        }


        public void SkinModify()
        {
            var mv = this.View as IMainView;

            ModifySkinDialogView.HeroList = this.HeroList;
            mv.ShowPopup(ModifySkinDialogView, "皮肤修改", false, false, true);
        }

        public void PlayGame()
        {
            string path = ShareSetting.Setting.LOLPath;

            string exePath = Path.Combine(path, "TCLS", "Client.exe");
            if (string.IsNullOrEmpty(exePath) || !File.Exists(exePath))
            {
                var dialog = new FolderBrowserDialog();
                dialog.Description = "运行本程序前，先设置LOL目录";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    path = dialog.SelectedPath;
                    ShareSetting.Setting.LOLPath = path;
                    OperateIniFile.WriteIniData("Game", "RootPath", path, ShareSetting.ConfigPath);
                    exePath = Path.Combine(path, "TCLS", "Client.exe");
                }
            }
            try
            {
                Process.Start(exePath);
            }
            catch (Exception ex)
            {
                var wm = ServiceManager.GetService<IWindowManager>();
                wm.ShowMessage(ex.Message);
            }
        }

        public void RunReplay()
        {
            string replayPath = Path.Combine(ShareSetting.ApplicationPath, "LOLReplay", "LOLRecorder.exe");
            Process.Start(replayPath);
        }



    }
}
