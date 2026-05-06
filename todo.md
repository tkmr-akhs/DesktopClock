# TODO

このファイルは、現時点で見つかっている未対応事項や設計上の検討事項をまとめたものです。

## タグ一覧

| タグ名 | 短縮タグ | 意味 |
| --- | --- | --- |
| バグ | `[bug]` | 現仕様・期待動作に対する誤動作。実務データで誤った結果になるもの。 |
| 要リファクタリング | `[ref]` | 動いてはいるが、設計・責務・依存関係・テスト容易性に問題があるもの。 |
| 仕様改善 | `[spec]` | 仕様の明確化、境界条件の整理、仕様上の矛盾、現仕様として明確だが著しく不便なものの見直し。 |
| 必須機能不足 | `[req]` | 実務利用に必要だが、現時点で仕様として欠けているもの。 |
| 利便性向上 | `[ux]` | 結果の見やすさ、調査しやすさ、操作性、運用しやすさの改善。 |
| 製品試験増強 | `[test]` | 製品試験の考慮不足など。 |

## 優先度の目安

- 高優先度: 誤動作、クラッシュ、リソースリーク、データ不整合、主要機能の使い勝手に直結するもの。
- 中優先度: すぐ壊れるとは限らないが、設計負債やテスト困難さとして早めに返済したいもの。
- 低優先度: 命名、運用整理、表示文言、将来の保守性改善が中心のもの。

## 高優先度

- [ ] `[ref]` 命名の不自然さとタイポを整理する。
  - `ObservableDictionalyTests`、`CommandExecuter`、`identifire`、`dispatcherQueueSerivce`、`textSyze`、`calenarWindowX`、`Utillity` などを修正する。
  - `Unauthenticate`、`IsAuthenticationRequired`、`needToAuthenticated` など、意味が取りづらい認証系の語を製品用語に合わせる。
  - `WindowAlignmentUnit` を文字サイズにも使っているため、`LengthUnit` など汎用名へ寄せるか、配置用/サイズ用を分ける。
  - `MinuteStyleSelectorService` の設定キーが `TimeStyleNumbers` / `TimeTextStyle` になっているなど、Hour/Minute/Date の命名規則を揃える。
  - ローカル変数 `Tomorrow` / `TomorrowCalEntry` などの PascalCase を通常の camelCase に直す。

- [ ] `[ux]` `[ref]` 生成ファイルとバックアップファイルの運用を整理する。
  - `.editorconfig` の BOM/CRLF ルールを `.csproj`、`.slnx`、`.md`、`.pu` にも広げる。
  - `.tmp` と `*.metaproj` は `.gitignore` で無視済みだが、実体が残るため掃除タイミングを決める。
  - `.bak` を無視するか、検証後に必ず掃除するかの方針を決める。
  - `DesktopClock.CustomAttributes` 配下に `Generated` / `bin` / `obj` だけが残っているため、必要なプロジェクトか生成物の残骸かを確認する。

- [ ] `[bug]` `[ref]` アプリ起動とウィンドウ生成の初期化順を明示する。
  - `App.MainWindow` が static 初期化で `new MainWindow()` される一方、`MainWindow` コンストラクターは `App.GetService` に依存しており、Host 構築順に暗黙依存している。
  - `MainWindow_Activated_FirstTime` と `ActivationService.InitializeAsync` に初期化責務が分散しているため、スタイル、DispatcherQueue、画面監視、サブウィンドウ生成の順序を 1 つの起動フローへ整理する。
  - UI スレッド前提の処理と `ConfigureAwait(false)` を混在させず、WinUI オブジェクトへ触れる処理は明示的に UI スレッドへ寄せる。
  - `DateTimeProviderService` のように DI 解決時点で動き始めるサービスは、起動/停止ライフサイクルを Host 側で管理する。
  - 終了時に `IHost` を破棄していないため、`LoggingService`、`DateTimeProviderService`、`WinFormsTrayIconService` など singleton の停止/破棄方針を統一する。

- [ ] `[bug]` `[spec]` 設定保存と設定 UI の型不一致を修正する。
  - `CalendarStyleSelectorService` は色を `Color` として保存している一方、読み込みは `string` のカラーコードとして読んでいるため、保存形式を統一する。
  - `CalendarStyleSelectorService` の public setter は `StyleChanged` 発火と保存を迂回できるため、変更経路を `Set...Async` に統一する。
  - `ChangeMarginUnitAsync`、`ChangeClockSizeUnitAsync`、`ChangeAlignmentAsync` は `int` 引数だが XAML から enum を渡しているため、コマンド引数を enum 型へ揃える。
  - `MainViewModel` コンストラクター内で `SetTextStyleAsync` を await せず呼び捨てている箇所を、初期化フローへ移す。
  - `MainViewModel` はフォント ファミリ/スタイル/太さ/色/縁取り色を公開しているが、設定 UI には高さと単位しかないため、UI を実装するか公開プロパティを削る。
  - `CalendarStyleSelectorService` には前景/背景/曜日/予定色の変更 API があるが、設定 UI から変更できないため、仕様として必要な色設定を整理する。
  - `TimeStyleSelectorServiceBase.NumbersSettingsKey` と `Numbers` は保存/読み込みにも UI にもつながっていないため、数字セットのカスタマイズを実装するか削除する。
  - Template Studio のままの About 文言、Privacy URL、`Unit of Hight Size` などの表示文字列を製品仕様に合わせる。

- [ ] `[bug]` `[ref]` `[test]` 時刻ソースを `IDateTimeProviderService` に統一する。
  - `ClockViewModel.RefreshHour/RefreshMinute`、`CalendarViewModel`、カレンダー用コンバーター、`MonthlyCalendar` などの `DateTime.Now` / `DateTime.Today` 直接参照を置き換える。
  - テスト可能性と日跨ぎ時の整合性を優先し、UI も同じ時刻スナップショットを参照する。
  - `HolidayDurationCalculator` の既定値も注入された日付または明示引数を使う形に寄せる。

- [ ] `[bug]` `[ref]` XAML コンバーターと文字画像生成の同期ブロックを整理する。
  - `GetImageAsync` / `GetBitmapAsync` が実質同期処理なのに `Task` を返し、コンバーター側で `.GetAwaiter().GetResult()` している構造を解消する。
  - 変換失敗時に `NotImplementedException` を投げるコンバーターは、`DependencyProperty.UnsetValue` や明示的な入力検証へ置き換える。
  - `DateInformationToImageConverterBase` と `CalendarEntryToDateTextImageConverter` で生成した `Bitmap` を破棄し、GDI リソース リークを避ける。
  - 文字画像生成、キャッシュ、スタイル選択の責務を分け、UI バインディング中の重い処理を減らす。

- [ ] `[ref]` `[test]` Google Calendar 同期の責務を分割する。
  - Google API アクセス、イベントからドメイン情報への変換、`MonthlyCalendar` への適用を分離する。
  - キャンセル、終日イベント範囲、時間指定イベント範囲、非表示カレンダー、祝日カレンダーまわりのテストを追加する。
  - 認証成功直後に `CalendarSettings` と Google カレンダー一覧を同期しないため、設定画面のカレンダー一覧が空または古いままになり得る。
  - Google 連携を無効化したときに `_calendarSettingsDictionary.Clear()` して保存するため、一時的な無効化で表示設定を失ってよいか仕様を決める。
  - `ApplyScheduleToMonthlyCalendar` の `complete` ループは実質 1 回で終わるため、再試行方針を明確にするか削除する。
  - 複数イベントが同じ日にある場合に `CalendarEntry.Information` が上書きされる挙動を、仕様として固定するか集約表示に変える。
  - `GoogleCalendarSetting.Equals` / `GetHashCode` が `Id` を見ていないため、同名同種の別カレンダーを同一扱いしてよいか確認する。

- [ ] `[spec]` `[ref]` Google 認証状態の名前と契約を見直す。
  - `IsAuthenticationRequired` は「認証済み」ではなく「Google 連携を使う設定」に近いため、UI 表示と名前を一致させる。
  - 設定キー `GooglePkceIsAuthenticated` と `BooleanToAuthenticationStatusConverter` の意味を揃える。
  - `IGooglePkceService` が `GooglePkceService.AuthenticationRequiredChangedEventArgs` という具象型に依存しないよう、イベント引数を契約側へ移す。

## 中優先度

- [ ] `[bug]` `[ref]` `DateTimeProviderService` を安全に停止できるようにする。
  - `MillisecondsInterval` の検証で、現在値ではなく代入される `value` を確認する。
  - `async void` の更新ループを、`PeriodicTimer` などを使った await 可能なループへ置き換える。
  - キャンセル例外が予期せず表に出ないよう、明示的な停止処理または破棄処理を追加する。

- [ ] `[bug]` `[ref]` `async void` の利用を減らし、イベント購読の寿命を管理する。
  - 非同期イベント処理は、ログ出力と例外処理を持つ await 可能な補助メソッドへ移す。
  - Transient な ViewModel が Singleton Service のイベントを購読したまま残らないよう、非アクティブ化時または破棄時に購読解除する。
  - 非同期コマンドには `AsyncRelayCommand` を優先して使う。
  - `ScreenChangedDetectionService.MonitorScreenChangesAsync` の無限ループに停止/破棄手段とキャンセルを追加する。
  - `DispatcherQueueService.InitializeAsync` より前に画面変更イベントが発生しても UI スレッド外で購読者を呼ばないよう、初期化順とフォールバックを見直す。
  - `ClockPage.HideWindowAndWaitPointerExit` の `async void`、手動 `Task`、`Thread.Sleep` ポーリング、複数起動時の表示/非表示競合を整理する。
  - `MonthlyCalendarService` の `CancellationTokenSource` をキャンセル後に破棄し、同時更新時の完了通知順を明確にする。

- [ ] `[bug]` `[test]` カレンダー ドメインの境界条件と通知を修正する。
  - `MonthlyCalendar.MaxDate` と `ContainsKey` の inclusive/exclusive がずれており、末尾日で範囲内判定なのにインデックス外になる可能性を潰す。
  - `MonthlyCalendar` は `IReadOnlyList<WeeklyCalendar>` と `IReadOnlyDictionary<DateOnly, CalendarEntry>` を同時に実装しているが、`Count` が週数を返すため辞書契約の件数と一致しない。
  - `MonthlyCalendar.Keys` / `Values` はパディング週を含む一方、`GetEnumerator()` は表示対象 6 週だけを返すため、公開コレクションの範囲を統一する。
  - `MonthlyCalendar.Clear` が各週を 7 回ずつ clear しているため、1 回だけ実行して不要な通知を減らす。
  - `MonthlyCalendar` は `INotifyPropertyChanging` を実装しているが、`PropertyChanging` を発火していないため契約に合わせる。
  - `HolidayDurationCalculator.CheckHolidayCore` は増加する `day` ではなく固定の `checkDay` で範囲確認しているため、範囲外参照を防ぐ。
  - `DateTimeRange.Includes(DateOnly)` と `DateOnlyRange` の half-open な日付範囲の意味をテストで固定し、Google 終日イベントの終了日 exclusive 仕様とずれないようにする。

- [ ] `[bug]` `[ux]` 複数モニターとウィンドウ配置の挙動を修正する。
  - `WindowAlignmentSelectorService.GetDisplayBounds` は有効な `ScreenId` でも常に 0 番目の画面を返す分岐になっているため、選択画面を正しく使う。
  - `ConvertToPixel` は `ScreenId` が範囲外のとき直接インデックス参照するため、画面抜き差し後も落ちないようにする。
  - `WindowAlignmentSetting.ScreenId` を設定 UI から変更できないため、保持している設定値とユーザー操作の関係を整理する。
  - `AdjustSize` と `SetRequestedAlignment` の hide/show 連続実行でちらつきや再入が起きないよう、配置更新をまとめる。

- [ ] `[ref]` `[test]` `ObservableDictionary` の同期モデルを整理する。
  - `_keyToIndex` 更新処理の TODO を解消し、挿入/削除/移動時の O(n) 再計算を仕様として許容するか、別データ構造へ寄せる。
  - `ObservableValues` からの Remove/Replace/Move と辞書本体からの操作で、イベント順と `_keyToIndex` の整合性を網羅テストする。
  - 同期状態が `Idle` 以外のときに操作を黙って無視する挙動を、例外・キューイング・再入禁止のどれにするか決める。
  - JSON 永続化で `ObservableDictionary<string, GoogleCalendarSetting>` を読み書きする前提をテストで固定する。

- [ ] `[bug]` `[ref]` ログ基盤の例外記録と契約を修正する。
  - `LoggingService.WriteLog` は `Log.Error(message, exception)` 形式になっており、Serilog の例外オーバーロードとして扱われないため、スタックトレースが記録される呼び方に直す。
  - `MinimumLevel.Debug()` のため `LogSeverity.Verbose` が出力されない設定になっている点を、仕様か設定ミスか決める。
  - `ILoggingService` が `DesktopClock.Services.LogSeverity` に依存しているため、契約側の enum に移す。
  - `WindowRepositoryService`、`CommandExecuter`、`RegistryHelper`、`ObservableDictionary.DebugWrite` などの `Debug.WriteLine` をログまたはテスト用の仕組みに寄せる。
  - ログ削除処理で `DateTime.Now` を直接使わず、保持期間の基準時刻をテスト可能にする。

- [ ] `[ref]` `[spec]` レジストリ/コマンド実行の Infra 境界を整理する。
  - `RegistryHelper` は `DesktopClock.Core` 配下にあるが namespace は `DesktopClock.Helpers` で、Windows/REG コマンド依存も強いため Infra 側へ移す。
  - `AutoStartSelectorService` で `Microsoft.Win32.Registry` と独自 `RegistryHelper` が混在しているため、1 つのポート/実装に統一する。
  - `RegistryKey.GetValueAsync/SetValueAsync` は null 名を許すコメントと実装が矛盾しているため、既定値の扱いを修正する。
  - `REG QUERY` 出力のパースが空白を含む値を扱えないため、実データに耐える取得方法へ変える。
  - `CommandExecuter` は `CommandExecutor` へ改名し、標準出力/標準エラー読み取りのデッドロック余地とタイムアウト未対応を潰す。

- [ ] `[ref]` DI とページ生成の境界を整理する。
  - `WindowRepositoryService.TryAddWindowOfPage<TPage>()` は `new TPage()` でページを生成しており、登録済み DI と ViewModel 注入を迂回している。
  - View、コンバーター、ヘルパーが `App.GetService` に直接依存しているため、Composition Root と Presentation の責務を分ける。
  - `PageService` のキーが ViewModel の `FullName` 文字列であるため、型安全なナビゲーション契約に寄せる。
  - `SubWindowHelper.ActiveWindows` が可変 `List` を公開しているため、Repository 経由で管理する。

- [ ] `[bug]` `[ref]` ローカル設定と Google `IDataStore` 実装の削除/同時更新の意味づけを整理する。
  - `LocalSettingsDataStoreService.DeleteAsync` と `ClearAsync` はキー削除ではなく default 値保存になっており、Google token の削除契約として正しいか確認する。
  - `ClearAsync` は現在プロセスで触れた `_keys` しか対象にしないため、過去に保存された token や設定が残る可能性を潰す。
  - `LocalSettingsService` は複数の `SaveSettingAsync` が同時に走ると `_settings` とファイル保存が競合し得るため、直列化または設定単位の保存方式を決める。
  - パッケージ版と非パッケージ版で保存先と JSON の入れ子構造が異なるため、移行/互換性をテストで固定する。

- [ ] `[ref]` レイヤ構成を App / Domain / Infra / Presentation の責務に合わせて再配置する。
  - `DesktopClock` プロジェクト内に Contracts / Services / Models / Helpers が混在しており、WinUI なしで App 層の振る舞いをテストしづらい。
  - `DesktopClock.Core` に未使用の `CommunityToolkit.Mvvm` 参照があり、Domain/Core が UI/MVVM 系依存を持たないように整理する。
  - `DesktopClock.Models` は `Windows.UI.Color` を含む表示設定、永続化 DTO、ドメイン寄り設定が混ざっているため、用途別に型を分ける。
  - `DesktopClock.Contracts.Services` と `DesktopClock.Core.Contracts.Services` の配置基準を決め、Presentation から App/Domain への依存方向を一貫させる。

- [ ] `[ux]` `[ref]` カレンダー画面の操作 UI とコマンド契約を整理する。
  - `CalendarPage.xaml` の `today<<` / `reload` がハードコード英語で、リソース化や表示文言の品質が設定画面と揃っていない。
  - `NextMonthCommand` / `PreviousMonthCommand` / `BackToThisMonthCommand` は非同期ラムダだが実質同期処理なので、`RelayCommand` / `AsyncRelayCommand` の使い分けを明確にする。
  - 更新中、認証未設定、同期失敗時の表示状態がないため、ユーザーが再読み込み結果を判断しづらい。

## 低優先度

- [ ] `[ref]` `[test]` nullable チェックとドキュメンテーション コメントを整備する。
  - `DesktopClock.Core` で nullable を有効化し、警告を段階的に潰す。
  - `RegistryHelper` の nullable 注釈警告と到達不能コードを修正する。
  - public メンバーの XML ドキュメント不足を補い、Domain/Core の public 未満メンバーにも必要な説明を加える。
  - `ImagingHelper.CombineBitmaps` のように非 nullable 戻り値で `null` を返し得る API を修正する。
  - `ClockViewModel` の `private　void` のような全角スペース混入を検出し、コード上必要のない不可視/非 ASCII 文字を除去する。

- [ ] `[ref]` Template Studio 由来の未使用コードと依存を整理する。
  - `SettingsStorageExtensions` は現在参照されておらず、`IsRoamingStorageAvailable` の戻り値も名前と逆に見えるため、使うなら修正し、使わないなら削除する。
  - `IReadOnlyReplaceableList<T>` など未使用の抽象を残すか削除するか決める。
  - `FrameExtensions.GetPageViewModel` の reflection 前提はナビゲーション契約整理後に不要化できるか確認する。

## TODO 整理メモ

- `設定保存と設定 UI の型不一致` は表示文言の課題も含むが、実行時挙動への影響が大きいため高優先度に置く。
- `nullable チェックとドキュメンテーション コメント` は重要だが、個別の実害は他の項目に分離済みのため低優先度に置く。
- `Google 認証状態の名前と契約` と `Google Calendar 同期の責務分割` は近いが、前者は認証状態の意味、後者はカレンダー一覧/予定同期の実行責務として分けて追跡する。
- `生成ファイルとバックアップファイルの運用` は `.tmp` / `*.metaproj` の ignore 設定自体は済んでいるため、残骸掃除と `.bak` 方針を主目的として残す。

## 対応を見送る事項 (無期ペンディング)

なし。

## 対応しないと決定した事項

なし。

## 対応済み事項

### 高優先度だったもの

- [x] `[ref]` Win32/DWM のウィンドウカスタマイズを `MainWindow` から分離する。
  - `DesktopClock.Win32` を Win32/API 統合境界として追加し、Native P/Invoke、ウィンドウ枠、デスクトップ所有者、透過背景、タイトルバー、通知領域アイコンをサービス化した。
  - `MainWindow` から DWM/User32/GDI32 の直接呼び出しを削除した。
  - `WindowChromeOptions` による汎用適用 API に寄せ、アプリ固有のウィンドウ種別を Win32 境界へ追加しなくて済む形にした。

- [x] `[ref]` `[spec]` `WinFormsWrapper` の責務と実装範囲を整理する。
  - `WinFormsWrapper` を `DesktopClock.Win32` に改名し、WinForms 依存を通知領域アイコンの実装サービス内へ閉じ込めた。
  - 未実装 public API と `System.Windows.Forms.Keys` の再定義を削除した。
  - `TrayIconOptions` / `TrayIconMenuItem` によるメニュー構成へ変更し、メニュー追加や並び替えでは Win32 実装を変更しなくて済む形にした。

### 中優先度だったもの

なし。

### 低優先度だったもの

なし。
