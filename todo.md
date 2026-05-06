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
  - `.bak`、`.tmp`、`.metaproj` を無視するか掃除するかの方針を決める。
  - `DesktopClock.CustomAttributes` 配下に `bin` / `obj` だけが残っているため、必要なプロジェクトか生成物の残骸かを確認する。

- [ ] `[ref]` Win32/DWM のウィンドウカスタマイズを `MainWindow` から分離する。
  - ウィンドウ枠、デスクトップ所有者、透過背景、通知アイコンの処理を、それぞれ焦点の絞られたサービスへ切り出す。
  - Presentation 層を薄く保ち、テストしやすい構造にする。
  - `MainWindow` から `App.GetService` によるサービスロケータ呼び出しを減らし、初期化順を DI で追えるようにする。

- [ ] `[ref]` `[spec]` `WinFormsWrapper` の責務と実装範囲を整理する。
  - `NotifyIcon.AddMenuItem(string, Stream, ...)` が public なのに未実装のため、実装するか削除する。
  - `NotifyIcon.Dispose` は `new` で隠すのではなく `Dispose(bool)` の override に寄せ、アイコン/画像/ストリームの破棄責務を明確にする。
  - `System.Windows.Forms.Keys` をほぼ丸ごと再定義しているため、必要最小限の型にするか WinForms 型を境界内へ閉じ込める。
  - WinForms 依存を Presentation から直接見せず、通知領域アイコン用のポートとして扱う。

- [ ] `[bug]` `[spec]` 設定保存と設定 UI の型不一致を修正する。
  - `CalendarStyleSelectorService` は色を `Color` として保存している一方、読み込みは `string` のカラーコードとして読んでいるため、保存形式を統一する。
  - `ChangeMarginUnitAsync`、`ChangeClockSizeUnitAsync`、`ChangeAlignmentAsync` は `int` 引数だが XAML から enum を渡しているため、コマンド引数を enum 型へ揃える。
  - `MainViewModel` コンストラクター内で `SetTextStyleAsync` を await せず呼び捨てている箇所を、初期化フローへ移す。
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
  - `MonthlyCalendar.Clear` が各週を 7 回ずつ clear しているため、1 回だけ実行して不要な通知を減らす。
  - `MonthlyCalendar` は `INotifyPropertyChanging` を実装しているが、`PropertyChanging` を発火していないため契約に合わせる。
  - `HolidayDurationCalculator.CheckHolidayCore` は増加する `day` ではなく固定の `checkDay` で範囲確認しているため、範囲外参照を防ぐ。

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

## 低優先度

- [ ] `[ref]` `[test]` nullable チェックとドキュメンテーション コメントを整備する。
  - `DesktopClock.Core` と `WinFormsWrapper` で nullable を有効化し、警告を段階的に潰す。
  - `RegistryHelper` の nullable 注釈警告と到達不能コードを修正する。
  - public メンバーの XML ドキュメント不足を補い、Domain/Core の public 未満メンバーにも必要な説明を加える。
  - `ImagingHelper.CombineBitmaps` のように非 nullable 戻り値で `null` を返し得る API を修正する。

## TODO 整理メモ

- `設定保存と設定 UI の型不一致` は表示文言の課題も含むが、実行時挙動への影響が大きいため高優先度に置く。
- `nullable チェックとドキュメンテーション コメント` は重要だが、個別の実害は他の項目に分離済みのため低優先度に置く。

## 対応を見送る事項 (無期ペンディング)

なし。

## 対応しないと決定した事項

なし。

## 対応済み事項

### 高優先度だったもの

なし。

### 中優先度だったもの

なし。

### 低優先度だったもの

なし。
