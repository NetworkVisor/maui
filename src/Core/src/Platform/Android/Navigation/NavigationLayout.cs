﻿using System;
using System.Collections.Generic;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using AndroidX.AppCompat.Widget;
using AndroidX.CoordinatorLayout.Widget;
using AndroidX.Navigation;
using AndroidX.Navigation.Fragment;
using Google.Android.Material.AppBar;

namespace Microsoft.Maui
{
	public class NavigationLayout : CoordinatorLayout, NavController.IOnDestinationChangedListener
	{
		NavHostFragment? _navHost;
		FragmentNavigator? _fragmentNavigator;
		Toolbar? _toolbar;
		AppBarLayout? _appBar;

		internal NavGraphDestination NavGraphDestination =>
			(NavGraphDestination)NavHost.NavController.Graph;

		internal IView? VirtualView { get; private set; }
		internal INavigationView? NavigationView { get; private set; }

		public IMauiContext MauiContext => VirtualView?.Handler?.MauiContext ??
			throw new InvalidOperationException($"MauiContext cannot be null");

#pragma warning disable CS0618 //FIXME: [Preserve] is obsolete
		[Preserve(Conditional = true)]
		public NavigationLayout(Context context) : base(context)
		{
		}

		[Preserve(Conditional = true)]
		public NavigationLayout(Context context, IAttributeSet attrs) : base(context, attrs)
		{
		}

		[Preserve(Conditional = true)]
		public NavigationLayout(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
		{
		}

		[Preserve(Conditional = true)]
		protected NavigationLayout(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{
		}
#pragma warning restore CS0618 //FIXME: [Preserve] is obsolete

		internal NavHostFragment NavHost
		{
			get => _navHost ?? throw new InvalidOperationException($"NavHost cannot be null");
			set => _navHost = value;
		}

		internal FragmentNavigator FragmentNavigator
		{
			get => _fragmentNavigator ?? throw new InvalidOperationException($"FragmentNavigator cannot be null");
			set => _fragmentNavigator = value;
		}


		internal Toolbar Toolbar
		{
			get => _toolbar ?? throw new InvalidOperationException($"ToolBar cannot be null");
			set => _toolbar = value;
		}

		internal AppBarLayout AppBar
		{
			get => _appBar ?? throw new InvalidOperationException($"AppBar cannot be null");
			set => _appBar = value;
		}


		public virtual void SetVirtualView(IView navigationView)
		{
			_toolbar = FindViewById<Toolbar>(Resource.Id.maui_toolbar);
			_appBar = FindViewById<AppBarLayout>(Resource.Id.appbar);

			VirtualView = navigationView;
			NavigationView = (INavigationView)navigationView;
		}

		internal void Connect()
		{
			var fragmentManager = Context?.GetFragmentManager();
			_ = fragmentManager ?? throw new InvalidOperationException($"GetFragmentManager returned null");
			_ = NavigationView ?? throw new InvalidOperationException($"VirtualView cannot be null");

			NavHost = (NavHostFragment)
				fragmentManager.FindFragmentById(Resource.Id.nav_host);

			FragmentNavigator =
				(FragmentNavigator)NavHost
					.NavController
					.NavigatorProvider
					.GetNavigator(Java.Lang.Class.FromType(typeof(FragmentNavigator)));

			var navGraphNavigator =
				(NavGraphNavigator)NavHost
					.NavController
					.NavigatorProvider
					.GetNavigator(Java.Lang.Class.FromType(typeof(NavGraphNavigator)));

			var navGraphSwap = new NavGraphDestination(navGraphNavigator);
			navGraphSwap.ApplyPagesToGraph(
				NavigationView.NavigationStack,
				this);

			NavHost.NavController.AddOnDestinationChangedListener(this);
			NavHost.ChildFragmentManager.RegisterFragmentLifecycleCallbacks(new FragmentLifecycleCallback(this), false);
		}

		protected private virtual void OnPageFragmentDestroyed(AndroidX.Fragment.App.FragmentManager fm, NavHostPageFragment navHostPageFragment)
		{
			_ = NavigationView ?? throw new InvalidOperationException($"NavigationView cannot be null");

			var graph = (NavGraphDestination)NavHost.NavController.Graph;
			NavigationView.NavigationFinished(graph.NavigationStack);
		}

		internal void ToolbarReady()
		{
			UpdateToolbar();
		}

		protected private virtual void UpdateToolbar()
		{

		}

		protected private virtual void OnFragmentResumed(AndroidX.Fragment.App.FragmentManager fm, NavHostPageFragment navHostPageFragment)
		{
		}

		public virtual void RequestNavigation(MauiNavigationRequestedEventArgs e)
		{
			var graph = (NavGraphDestination)NavHost.NavController.Graph;
			graph.ApplyNavigationRequest(e.NavigationStack, e.Animated, this);
		}

		internal void OnPop()
		{
			_ = NavigationView ?? throw new InvalidOperationException($"NavigationView cannot be null");

			var graph = (NavGraphDestination)NavHost.NavController.Graph;
			var stack = new List<IView>(graph.NavigationStack);
			stack.RemoveAt(stack.Count - 1);
			graph.ApplyNavigationRequest(stack, true, this);
		}

		#region IOnDestinationChangedListener
		void NavController.IOnDestinationChangedListener.OnDestinationChanged(
			NavController p0, NavDestination p1, Bundle p2)
		{
			if (p1 is FragmentNavDestination fnd)
			{
				var titledElement = fnd.Page as ITitledElement;
				Toolbar.Title = titledElement?.Title;
			}

			ToolbarReady();
		}
		#endregion



		class FragmentLifecycleCallback : AndroidX.Fragment.App.FragmentManager.FragmentLifecycleCallbacks
		{
			NavigationLayout _navigationLayout;

			public FragmentLifecycleCallback(NavigationLayout navigationLayout)
			{
				_navigationLayout = navigationLayout;
			}


			public override void OnFragmentResumed(AndroidX.Fragment.App.FragmentManager fm, AndroidX.Fragment.App.Fragment f)
			{
				if (f is NavHostPageFragment pf)
					_navigationLayout.OnFragmentResumed(fm, pf);
			}

			public override void OnFragmentAttached(AndroidX.Fragment.App.FragmentManager fm, AndroidX.Fragment.App.Fragment f, Context context)
			{
				base.OnFragmentAttached(fm, f, context);
			}

			public override void OnFragmentViewDestroyed(
				AndroidX.Fragment.App.FragmentManager fm,
				AndroidX.Fragment.App.Fragment f)
			{
				if (f is NavHostPageFragment pf)
					_navigationLayout.OnPageFragmentDestroyed(fm, pf);

				base.OnFragmentViewDestroyed(fm, f);
			}
		}

	}
}
