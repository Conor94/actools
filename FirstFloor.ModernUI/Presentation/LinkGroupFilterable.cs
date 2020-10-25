using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using FirstFloor.ModernUI.Helpers;

namespace FirstFloor.ModernUI.Presentation {
    public class LinkGroupFilterable : LinkGroup {
        private string KeyGroup => "lgf_" + _source;

        private string KeySelected => ".lgf.s_" + _source;

        private string KeyRecentlyClosed => ".lgf.rc_" + _source;

        private bool _addAllLink = true;

        private Lazy<string> _filterHint;

        public Lazy<string> FilterHint {
            get => _filterHint;
            set => Apply(value, ref _filterHint);
        }

        /// <summary>
        /// Set it before setting Source!
        /// </summary>
        public bool AddAllLink {
            get => _addAllLink;
            set => Apply(value, ref _addAllLink);
        }

        private bool _initialized;
        private Uri _selectLater;

        public override void Initialize() {
            if (_initialized) return;
            _initialized = true;

            Links.Clear();
            if (AddAllLink) {
                Links.Add(new Link {
                    DisplayName = UiStrings.FiltersLinkAll,
                    Source = _source
                });
            }

            foreach (var link in from x in ValuesStorage.GetStringList(KeyGroup)
                where !string.IsNullOrWhiteSpace(x)
                select new LinkInput(_source, x)) {
                Links.Add(link);
                link.PropertyChanged += Link_PropertyChanged;
                link.Close += OnLinkClose;
            }

            var rightLink = new LinkInputEmpty(Source);
            Links.Add(rightLink);
            rightLink.NewLink += OnNewLink;

            RecentlyClosedQueue.AddRange(ValuesStorage.GetStringList(KeyRecentlyClosed));
            LoadSelected();
        }

        private void LoadSelected() {
            var source = ValuesStorage.Get<string>(KeySelected);
            SetSelected((_selectLater == null ?
                    Links.FirstOrDefault(x => x.DisplayName == source) :
                    Links.FirstOrDefault(x => x.Source == _selectLater)) ?? Links.FirstOrDefault(), false);
            _selectLater = null;
        }

        public void SetSelected(Uri uri) {
            if (_initialized) {
                SetSelected(Links.FirstOrDefault(x => x.Source == uri) ?? Links.FirstOrDefault(), false);
            } else {
                _selectLater = uri;
            }
        }

        private void SetSelected(Link value, bool save) {
            if (_selectedLink == value) return;
            if (_selectedLink != null) {
                PreviousSelectedQueue.Remove(value);
                PreviousSelectedQueue.Enqueue(value);
            }

            _selectedLink = value;
            OnPropertyChanged(nameof(SelectedLink));

            if (save && value?.Source != null && value.IsTemporary != true) {
                ValuesStorage.Set(KeySelected, value.DisplayName);
            }
        }

        private Link _selectedLink;

        public override Link SelectedLink {
            get => _selectedLink;
            set => SetSelected(value, true);
        }

        private void SaveLinks() {
            ValuesStorage.Storage.SetStringList(KeyGroup, from x in Links where x is LinkInput select x.DisplayName);
        }

        public void OnDrop(LinkInput widget, int newIndex) {
            if (newIndex == -1) {
                newIndex = Links.Count - 1;
            } else {
                var minIndex = FixedLinksCount;
                if (newIndex < minIndex) {
                    newIndex = minIndex;
                }
            }

            Links.Remove(widget);
            Links.Insert(newIndex, widget);
            SaveLinks();
        }

        private void SaveRecentlyClosed() {
            ValuesStorage.Storage.SetStringList(KeyRecentlyClosed, RecentlyClosedQueue);
        }

        public static int OptionRecentlyClosedQueueSize = 10;

        private LimitedQueue<Link> PreviousSelectedQueue { get; } = new LimitedQueue<Link>(10);
        private LimitedQueue<string> RecentlyClosedQueue { get; } = new LimitedQueue<string>(OptionRecentlyClosedQueueSize);

        public void RestoreLastClosed() {
            if (!RecentlyClosedQueue.Any()) return;
            AddAndSelect(RecentlyClosedQueue.Dequeue());
            SaveRecentlyClosed();
        }

        public event EventHandler<LinkChangedEventArgs> LinkChanged;

        private void Link_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName != nameof(LinkInput.DisplayName)) return;

            var link = sender as LinkInput;
            if (link == null) return;

            LinkChanged?.Invoke(this, new LinkChangedEventArgs(link.PreviousValue, link.DisplayName));
            if (ReferenceEquals(link, SelectedLink) && !link.IsTemporary) {
                ValuesStorage.Set(KeySelected, link.DisplayName);
            }

            var sameValue = Links.OfType<LinkInput>().FirstOrDefault(x => x.DisplayName == link.DisplayName && x != link);
            if (sameValue != null) {
                Remove(sameValue);
            }

            SaveLinks();
        }

        public void AddAndSelect(string value) {
            var sameValue = Links.OfType<LinkInput>().FirstOrDefault(x => x.DisplayName == value);
            if (sameValue != null) {
                SelectedLink = sameValue;
                return;
            }

            var link = new LinkInput(_source, value);
            link.PropertyChanged += Link_PropertyChanged;
            link.Close += OnLinkClose;

            Links.Insert(Links.Count - 1, link);
            SelectedLink = link;

            SaveLinks();
        }

        private void Remove(LinkInput link) {
            PreviousSelectedQueue.Remove(link);

            link.PropertyChanged -= Link_PropertyChanged;
            link.Close -= OnLinkClose;

            if (SelectedLink == link) {
                SelectedLink = PreviousSelectedQueue.DequeueOrDefault() ?? Links[Links.IndexOf(link) - 1];
            }

            if (SelectedLink == link) {
                SelectedLink = Links.FirstOrDefault();
            }

            Links.Remove(link);
            SaveLinks();
        }

        private void OnNewLink(object sender, NewLinkEventArgs e) {
            AddAndSelect(e.InputValue);
        }

        private void OnLinkClose(object sender, LinkCloseEventArgs e) {
            if (sender is LinkInput link) {
                IEnumerable<LinkInput> listToClose;
                switch (e.Mode) {
                    case LinkCloseMode.Regular:
                        listToClose = new[] { link };
                        break;
                    case LinkCloseMode.CloseAll:
                        listToClose = Links.OfType<LinkInput>();
                        break;
                    case LinkCloseMode.CloseOthers:
                        listToClose = Links.OfType<LinkInput>().Where(x => x != link);
                        break;
                    case LinkCloseMode.CloseToRight:
                        listToClose = Links.OfType<LinkInput>().SkipWhile(x => x != link).Skip(1);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                foreach (var toClose in listToClose.ToList()) {
                    Remove(toClose);
                    RecentlyClosedQueue.Enqueue(toClose.DisplayName);
                }
                SaveRecentlyClosed();
            }
        }

        private bool _isEnabled = true;

        public bool IsEnabled {
            get => _isEnabled;
            set => Apply(value, ref _isEnabled);
        }

        private Uri _source;

        public Uri Source {
            get => _source;
            set {
                if (_source == value) return;
                _source = value;
                OnPropertyChanged();

                Initialize();
            }
        }

        private class InnerFixedList : IList {
            private readonly LinkGroupFilterable _parent;
            private readonly int _skip;

            private LinkCollection Links => _parent.Links;

            public InnerFixedList(LinkGroupFilterable parent, bool noAll) {
                _parent = parent;
                _skip = noAll ? 0 : 1;
            }

            public IEnumerator GetEnumerator() {
                return (_skip > 0 ? Links.Skip(_skip) : Links).Take(Count).GetEnumerator();
            }

            public void CopyTo(Array array, int index) {
                (_skip > 0 ? Links.Skip(_skip) : Links).Take(Count).ToArray().CopyTo(array, index);
            }

            public int Count { get; private set; }

            public object SyncRoot { get; } = new object();

            public bool IsSynchronized => false;

            public int Add(object value) {
                Links.Insert(Count + _skip, (Link)value);
                _parent.LoadSelected();
                return Count++;
            }

            public bool Contains(object value) {
                return (_skip > 0 ? Links.Skip(_skip) : Links).Take(Count).Contains(value);
            }

            public void Clear() {
                while (Count > 0) {
                    RemoveAt(0);
                }
            }

            public int IndexOf(object value) {
                var r = 0;
                foreach (var source in (_skip > 0 ? Links.Skip(_skip) : Links).Take(Count)) {
                    if (ReferenceEquals(source, value)) return r;
                    r++;
                }
                return -1;
            }

            public void Insert(int index, object value) {
                if (index < 0 || index > Count) throw new ArgumentOutOfRangeException(nameof(index));
                Links.Insert(index + _skip, (Link)value);
            }

            public void Remove(object value) {
                if (Contains(value)) {
                    Links.Remove((Link)value);
                    Count--;
                }
            }

            public void RemoveAt(int index) {
                if (index < 0 || index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
                Links.RemoveAt(index + _skip);
                Count--;
            }

            public object this[int index] {
                get {
                    if (index < 0 || index > Count) throw new ArgumentOutOfRangeException(nameof(index));
                    return Links[index + _skip];
                }
                set {
                    if (index < 0 || index > Count) throw new ArgumentOutOfRangeException(nameof(index));
                    Links[index + _skip] = (Link)value;
                }
            }

            public bool IsReadOnly => false;

            public bool IsFixedSize => false;
        }

        public int FixedLinksCount => (_fixedLinks?.Count ?? 0) + (AddAllLink ? 1 : 0);

        private InnerFixedList _fixedLinks;

        public IList FixedLinks => _fixedLinks ?? (_fixedLinks = new InnerFixedList(this, !AddAllLink));
    }
}