﻿using System;
using System.Drawing;
using System.Collections.Generic;
using FFImageLoading.Work;
using CoreGraphics;

#if __IOS__
using Foundation;
using UIKit;
#elif __ANDROID__
using Android.Util;
using Android.Runtime;
using Android.Content;
using FFImageLoading.Views;
#endif

namespace FFImageLoading.Cross
{
	[Register("MvxImageLoadingView")]
	public class MvxImageLoadingView
	#if __IOS__
		: UIImageView
	#elif __ANDROID__
		: ImageViewAsync
	#endif
	{
		#if __IOS__
		public MvxImageLoadingView(IntPtr handle) : base(handle) { Initialize(); }
		public MvxImageLoadingView(CGRect frame) : base(frame) { Initialize(); }
		#elif __ANDROID__
		public MvxImageLoadingView(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer) { Initialize(); }
		public MvxImageLoadingView(Context context, SizeF? predefinedSize = null) : base(context) { Initialize(); }
		public MvxImageLoadingView(Context context, IAttributeSet attrs, SizeF? predefinedSize) : base(context, attrs) { Initialize(); }
		public MvxImageLoadingView(Context context, IAttributeSet attrs) : base(context, attrs) { Initialize(); }
		#endif

		private TaskParameter _parameters;
		private string _dataLocation;

		private void Initialize()
		{
			LoadingPlaceholderSource = ImageSource.Filepath;
			ErrorPlaceholderSource = ImageSource.Filepath;
		}

		public ImageSource? Source { get; set; }

		public TimeSpan? CacheDuration { get; set; }

		public Tuple<int, int> DownSampleSize { get; set; }

		public bool DownSampleUseDipUnits { get; set; }

		public InterpolationMode DownSampleInterpolationMode { get; set; }

		public ImageSource LoadingPlaceholderSource { get; set; }

		public string LoadingPlaceholderPath { get; set; }

		public ImageSource ErrorPlaceholderSource { get; set; }

		public string ErrorPlaceholderPath { get; set; }

		public int RetryCount { get; set; }

		public int RetryDelayInMs { get; set; }

		public Action<ImageSize, LoadingResult> OnSuccess { get; set; }

		public Action<Exception> OnError { get; set; }

		public Action<IScheduledWork> OnFinish { get; set; }

		public List<ITransformation> Transformations { get; set; }

		public bool? LoadTransparencyChannel { get; set; }

		public bool? FadeAnimationEnabled { get; set; }

		public bool? TransformPlaceholdersEnabled { get; set; }

		public string CustomCacheKey { get; set; }

		public string DataLocation
		{
			get
			{
				return _dataLocation;
			}
			set
			{
				if (Source == null)
					throw new Exception("ImageSource must be defined prior to define DataLocation.");

				_dataLocation = value;

				CleanParameters();
				_parameters = MakeParams();
				_parameters.Into(this);
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (_parameters != null)
				{
					CleanParameters();
				}
			}
			base.Dispose(disposing);
		}

		private void CleanParameters()
		{
			_parameters.Dispose();
			_parameters = null;
			OnSuccess = null;
			OnError = null;
			OnFinish = null;
		}

		private TaskParameter MakeParams()
		{
			var parameters = InstanciateParams(Source.Value, DataLocation, CacheDuration);

			if (DownSampleSize != null)
			{
				if (DownSampleUseDipUnits)
				{
					parameters.DownSampleInDip(DownSampleSize.Item1, DownSampleSize.Item2);
				}
				else
				{
					parameters.DownSample(DownSampleSize.Item1, DownSampleSize.Item2);
				}
			}

			if (LoadingPlaceholderPath != null)
			{
				parameters.LoadingPlaceholder(LoadingPlaceholderPath, LoadingPlaceholderSource);
			}
			if (ErrorPlaceholderPath != null)
			{
				parameters.LoadingPlaceholder(ErrorPlaceholderPath, ErrorPlaceholderSource);
			}

			parameters.DownSampleMode(DownSampleInterpolationMode);
			parameters.Retry(RetryCount, RetryDelayInMs);

			if (OnSuccess != null)
			{
				parameters.Success(OnSuccess);
			}
			if (OnError != null)
			{
				parameters.Error(OnError);
			}
			if (OnFinish != null)
			{
				parameters.Finish(OnFinish);
			}

			if (Transformations != null)
			{
				parameters.Transform(Transformations);
			}

			if (LoadTransparencyChannel.HasValue)
			{
				parameters.TransparencyChannel(LoadTransparencyChannel.Value);
			}

			if (FadeAnimationEnabled.HasValue)
			{
				parameters.FadeAnimation(FadeAnimationEnabled.Value);
			}

			if (TransformPlaceholdersEnabled.HasValue)
			{
				parameters.TransformPlaceholders(TransformPlaceholdersEnabled.Value);
			}

			if (CustomCacheKey != null)
			{
				parameters.CacheKey(CustomCacheKey);
			}

			return parameters;
		}

		private TaskParameter InstanciateParams(ImageSource source, string dataLocation, TimeSpan? cacheDuration)
		{
			switch (source)
			{
				case ImageSource.ApplicationBundle:
					return TaskParameter.FromApplicationBundle(dataLocation);

				case ImageSource.CompiledResource:
					return TaskParameter.FromCompiledResource(dataLocation);

				case ImageSource.Filepath:
					return TaskParameter.FromFile(dataLocation);

				case ImageSource.Stream:
					throw new Exception("ImageSource Stream is not supported by MvxImageLoadingView");

				case ImageSource.Url:
					return TaskParameter.FromUrl(dataLocation, cacheDuration);

				default:
					throw new Exception("Invalid parameters supplied to InstanciateParams");
			}
		}
	}
}
