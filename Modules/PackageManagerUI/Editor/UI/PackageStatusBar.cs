// Unity C# reference source
// Copyright (c) Unity Technologies. For terms of use, see
// https://unity3d.com/legal/licenses/Unity_Reference_Only_License

using System;
using UnityEngine.UIElements;

namespace UnityEditor.PackageManager.UI
{
    internal class PackageStatusBar : VisualElement
    {
        internal new class UxmlFactory : UxmlFactory<PackageStatusBar> {}

        internal static readonly string k_OfflineErrorMessage = "You seem to be offline";

        private enum StatusType { Normal, Loading, Error }

        public PackageStatusBar()
        {
            var root = Resources.GetTemplate("PackageStatusBar.uxml");
            Add(root);
            cache = new VisualElementCache(root);
        }

        public void OnEnable()
        {
            UpdateStatusMessage();

            PageManager.instance.onRefreshOperationStart += UpdateStatusMessage;
            PageManager.instance.onRefreshOperationFinish += UpdateStatusMessage;
            PageManager.instance.onRefreshOperationError += OnRefreshOperationError;

            PackageFiltering.instance.onFilterTabChanged += OnFilterTabChanged;
            ApplicationUtil.instance.onInternetReachabilityChange += OnInternetReachabilityChange;

            refreshButton.clickable.clicked += () =>
            {
                if (!EditorApplication.isPlaying)
                    PageManager.instance.Refresh();
            };
        }

        public void OnDisable()
        {
            PageManager.instance.onRefreshOperationStart -= UpdateStatusMessage;
            PageManager.instance.onRefreshOperationFinish -= UpdateStatusMessage;
            PageManager.instance.onRefreshOperationError -= OnRefreshOperationError;

            PackageFiltering.instance.onFilterTabChanged -= OnFilterTabChanged;
            ApplicationUtil.instance.onInternetReachabilityChange -= OnInternetReachabilityChange;
        }

        private void OnInternetReachabilityChange(bool value)
        {
            UpdateStatusMessage();
        }

        private void OnFilterTabChanged(PackageFilterTab tab)
        {
            UpdateStatusMessage();
        }

        private void OnRefreshOperationError(Error error)
        {
            UpdateStatusMessage();
        }

        private void UpdateStatusMessage()
        {
            var tab = PackageFiltering.instance.currentFilterTab;

            if (PageManager.instance.IsRefreshInProgress(tab))
            {
                SetStatusMessage(StatusType.Loading, L10n.Tr("Refreshing packages..."));
                return;
            }

            var errorMessage = string.Empty;
            if (!ApplicationUtil.instance.isInternetReachable)
                errorMessage = L10n.Tr(k_OfflineErrorMessage);
            else if (PageManager.instance.GetRefreshError(tab) != null)
                errorMessage = L10n.Tr("Error refreshing packages, see console");

            if (!string.IsNullOrEmpty(errorMessage))
            {
                SetStatusMessage(StatusType.Error, errorMessage);
                return;
            }

            var timestamp = PageManager.instance.GetRefreshTimestamp(tab);
            var label = timestamp == 0L ? string.Empty : L10n.Tr($"Last update {new DateTime(timestamp):MMM d, HH:mm}");
            SetStatusMessage(StatusType.Normal, label);
        }

        private void SetStatusMessage(StatusType status, string message)
        {
            if (status == StatusType.Loading)
                loadingSpinner.Start();
            else
                loadingSpinner.Stop();

            UIUtils.SetElementDisplay(errorIcon, status == StatusType.Error);
            statusLabel.text = message;
        }

        private VisualElementCache cache { get; set; }

        private LoadingSpinner loadingSpinner { get { return cache.Get<LoadingSpinner>("loadingSpinner"); }}
        private Label errorIcon { get { return cache.Get<Label>("errorIcon"); }}
        private Label statusLabel { get { return cache.Get<Label>("statusLabel"); }}
        private Button refreshButton { get { return cache.Get<Button>("refreshButton"); } }
    }
}
