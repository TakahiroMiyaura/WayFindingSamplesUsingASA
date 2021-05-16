# WayFindingSamplesUsingASA
this Sample is realize the 'way-finding' demo using Azure Spatial Anchors.

[English](./README_en.md)

## Azure Spatial Anchorsのユースケース「Way-Finding」を実現するためのテクニック

de:code 2020 のセッション内容をより深く理解し実践するのに役に立つサンプルコードやツールを提供ということで、Azure Spatial Anchorsの１つのユースケース「Way-Finding」を
実現するテクニックを紹介します。

![Demo](./images/WayfindingUsingASA.gif)

このアプリケーションはAzure Spatial Anchorsを活用して以下のコンテンツを実現します。

* 基点になるAnchorを「Azure Spatial Anchors」で構築し、IDをローカルに保存します。
   * 2回目以降は保存済みのアンカーIDでアンカーの取得を行います。
* 次にルート設定のためのアンカーを設置します。
   * アンカーは中継点になる「Point」と終点となる「Destination」の2種類を設定します。
   * 中継点はいくつ配置しても問題ないのですが、アンカーの探知範囲（デフォルト5m）よりも遠くに設定すると発見できなくなります。
   * 終点には「場所の名前」が設定可能です（フォントの関係で英字のみ）
   * ルートは複数登録可能ですが、一度アプリを終了する必要があります。
* ルートに従って目的地まで移動してみましょう。
   * 基点のアンカー設置後に行先リストが表示されます。
   * 行きたい場所を選択すると、基点から近いアンカーの探索を自動的に行います。
   * 以降表示されるアンカーに近づくと付近のアンカーを探索します。 

### 開発環境

このサンプルは以下の環境の下で作成、動作確認をしています。

#### ハード
* PC
  * Windows 10 Pro(OSバージョン 20H2)
* HoloLens 2(HoloLensでも動作可)

#### ソフトウェア
* [Unity 2019.4.X LTS](https://unity3d.com/jp/get-unity/download/archive)(リポジトリではUnity 2019.4.19f1を使用)
* [Visual Studio 2019(16.5.3)](https://visualstudio.microsoft.com/ja/downloads/)
* Azure Spatial anchors 2.8.1(Mixed Reality Feature Toolでセットアップ)
* [Mixed Reality Toolkit 2.3.0](https://github.com/microsoft/MixedRealityToolkit-Unity/releases/tag/v2.3.0)
* [Mixed Reality Feature Tool](https://www.microsoft.com/en-us/download/details.aspx?id=102778)

### 開発に必要な前準備
#### Azure Spatial Anchorsの準備
このサンプルはZure Spatial Anchorのクイックスタートと導入手順は同じになります。前提条件、開発に必要なAzureの構築は以下のサイトを参照してください。  
[クイック スタート:Azure Spatial Anchors を使用する HoloLens Unity アプリを作成する](https://docs.microsoft.com/ja-jp/azure/spatial-anchors/quickstarts/get-started-unity-hololens)

#### 各種モジュールのダウンロード

ソフトウェアに記載の通り、このサンプルはいくつかのライブラリを利用します。
現時点のバージョンは上記の通りですので各サイトから必要なモジュールをダウンロードしてください。
Azure Spatial Anchorsの導入には「[Mixed Reality Feature Tool](https://www.microsoft.com/en-us/download/details.aspx?id=102778)」を使用します。
サイトからダウンロードし、zipファイルを展開しておきます。

* [(公式)Mixed Reality Feature Tool へようこそ](https://docs.microsoft.com/ja-jp/windows/mixed-reality/develop/unity/welcome-to-mr-feature-tool)

### 利用手順

1. Gitからこのリポジトリをクローンまたはダウンロードする。
2. [Mixed Reality Feature Tool](https://www.microsoft.com/en-us/download/details.aspx?id=102778)を利用してAzure Spatial Anchorsを導入します。(Mixed Reality Feature ToolでもMRTKはセットアップ可能ですが、使用バージョンをセットアップできないため選択しないでください。)
    1. MixedRealityFeatureTool.exeをダブルクリックして実行します。
    2. [Start]をクリックします。
    3. [Select project]セクションで手順1.でクローンしたリポジトリの中の[WayFindingSamplesUsingASA\Unity]を設定し[Discover Features]をクリックします。
    4. [Discover features]セクションで以下のAzure Spatial Anchorsに関連するサービスにチェックを入れ、[Get Features]をクリックします。
        * Azure Spatial Anchors SDK for Android 2.8.1
        * Azure Spatial Anchors SDK Core 2.8.1
        * Azure Spatial Anchors SDK for iOS 2.8.1
        * Azure Spatial Anchors SDK for Windows 2.8.1
    5. [Import features]セクションで先ほど選択した4つのコンポーネントが選択されていることを確認し[Import]をクリックします。
    6. [Review and Approve]セクションでは[Approve]をクリックして適用します。
    7. [Exit]をクリックしてアプリケーションを終了します。
2. WayFindingSamplesUsingASA\UnityをUnityで開きます。
3. [Asset]-[Import Package]-[Custom Package]を開きあらかじめダウンロードした[Microsoft.MixedReality.Toolkit.Unity.Foundation.2.3.0.unitypackage](https://github.com/microsoft/MixedRealityToolkit-Unity/releases/tag/v2.3.0) をインポートします。
4. [MRTK Project Configurator]の項目をすべてチェックして適用します。
5. WayFindingSamplesUsingASA\Unity\Assets\ASA.Samples.WayFindings\Scenes\SampleScene.unityを開く
6. Projectパネルの[AzureSpatialAnchors.SDK\Resources\SpatialAnchorConfig]を選択し、Inspectorパネル内の[Credentials]にAccount IDとKey、domainを設定します。
7. [File]-[Build Settings]を開き、以下の設定を行います。
    * PlatformをUniversal Windows Platformに変更する
    * 上部メニューの[Mixed Reality Toolkit]-[Utilities]-[Configure Unity Project]を選択しダイアログのチェックがすべてOKになるように設定する。
    * [Edit]-[Project Settings]を開き[Player]カテゴリの以下の設定をすべて変更する
       * [XR Settings]で[Virtual Reality Supported]にチェック
       * [XR Settings]で[Virtual Reality SDKs]で[Windows Mixed Reality]を追加し、[Depth Format]を16bitに変更する
       * [Publish Settings]の[Capabilities]で以下の設定にチェックが入っていることを確認する。
           * InternetClient
           * InternetClientServer
           * RemovableStorage
           * SpatialPerception 
8. ビルドを実施し任意のフォルダにビルド資産を展開します。
9. ビルドが完了後Visual Studioでソリューションファイルを開き、HoloLensにデプロイします。

※リポジトリのunitypackageフォルダにはGithub上で実装した部分のみをエクスポートしたものです。必要に応じて利用してください。


### 動作について

HoloLensにデプロイしたアプリケーションを開きます。動作は以下の流れとなります。

    1. 基点になるアンカーの設置を行う。
        1.  初回起動時
            1.  空間にあるオレンジ色のキューブを基点にしたい場所に配置します。
            2.  Azure Spatial Anchorsのセションを開始→Anchorの作成ボタンを押してアンカーの登録を行います。正常に登録が完了すると自動的にローカルストレージにアンカーIDが保存されます。
        2.  2回目以降
            1. 前回登録時のアンカーIDをストレージからロードします。
            2. Azure Spatial Anchorsのセションを開始→Anchor検索のボタンを押してアンカーの検索を行います。
        3. アンカーが正常に設置できると次の2つのいずれかを選択できます。
    2. ルートを設定する
        1. 経路と目的地を設定します。途中の経路については次のアンカーポイントを作成するボタンを押して青色のキューブを設置したい場所に配置し、Anchorの作成ボタンを押します。
        2. 経路の設定が完了したら最後に目標地の設定ボタンを押します。途中経路のものとは異なりテキスト入力フィールドが表示されているので、任意の位置に配置後テキストボックスをタップして目的地名を入力してください（半角文字のみ）。
        3. Anchorの作成ボタンを押すと、Anchorの登録とともに、今まで設定した経路上のアンカーすべてに目的地名が反映されます。
    3. 経路を探索する
        1. 目的地の一覧が表示されるので、行先を選択します。
        2. 基点から一定範囲内の経路上のアンカーが探索され可視化されます。
        3. アンカーに一定距離近づくと、近づいたアンカーを中心に次のアンカーを探索します。
        4. 3を目的地まで繰り返して移動します。 

### 技術的なポイント
Azure Spatial Anchorはクラウド上にアンカーとその周辺の空間情報を持つことでクロスプラットフォームで空間情報を活用したコンテンツ共有が可能です。
利用する上で知っておくとよいと思う機能を紹介します。

#### アンカーの登録とリンク

Azure Spatial Anchorsは最初に設定したアカウントIDとキーでサービスにアクセスします。Spatial Anchorを利用するためには、最初にサービスとの接続を行いセションを開きます。
このセションは一般的なセションと同じで一定時間サービスを利用する事ができます。
セションを開いた後、オブジェクトを現実空間の任意の場所に配置します。

配置後Azure Spatial Anchorsの情報として利用できるアンカー情報をオブジェクトに設定し、登録処理を実施します。

正常に登録が完了すると戻り値としてGUID形式のAnchorIDが返却されます。以降アンカーの取得にはこのIDを利用します。

同一セション内で登録/取得されたアンカー同士は情報がリンクされた状態になります。この状態で、例えばある1つのアンカーを基準にして半径5m圏内のアンカーを検索するなどが可能になります。
あくまで同一セション内で登録/取得されたアンカーのみがリンクの対象となるため、「新しいアンカーAを作成後、Aの周辺のアンカーを探索する」ことはできません。

これは、Azure Spatial Anchors上で登録されているアンカーと新しいアンカーAにリンクが持たれていないためです。このような場合は、今回のサンプルのように1つ目はAzure Spatial Anchorsから取得し、その後同一セション内でアンカー登録を行うことでリンクが保たれます。

（なお、Azure Spatial Anchors V2.0.0から各種センサーの情報をID代わりにしてアンカーを取得することも可能です。この場合は、Spatial Anchorの登録時にセンサー情報を埋め込む必要があります。）

#### アンカーの取得（IDと近接のアンカー）

アンカーの取得には「IDによる検索」、「すでに取得済みのアンカー周辺の検索」、「センサーデバイス（Wifi,Bluetooth,GPS）周辺の検索」の3つがあります。今回はこの中で前者2つの方法を利用しています。

最初に「IDによる検索」です。基点のアンカーを登録しますが、この時のIDはローカルストレージで永続化しています。このため、2回目以降の起動では、永続化したアンカーIDを使ってSpatial Anchorを取得配置しています。

「すでに取得済みのアンカー周辺の検索」は経路の登録済みの状態で基点のアンカー周辺の次地点のアンカーを取得しています。これを準備繰返しして経路を構成しています。

#### 道案内に必要な情報

上記のようにあAzure Spatial Anchorsは基点になる1つでもAnchorをクラウドから取得できれば、Spatial Anchor自体を目印に道案内が可能です。とはいえ、目的地点やそこまでの経路につながる情報を持たせない限りは道案内のルートとして伝えることができません。
このため、アンカーのID情報とそのアンカー群からなる情報で目的地となる座標、経路の情報を何か別の仕組みで持たせる必要があります。特に建物内の最短経路などを探索させる場合はアンカー同士の関係からアルゴリズムを適用して実現することになります。

このための簡易な手段の1つとして、Azure Spatial Anchorsはアンカーの情報（ID,や空間の情報）以外にstring型の辞書を持つことができます。例えば、アンカー登録時に目標点となるアンカーには目的地の名称を登録しておくことで、アンカー取得時にその情報を利用してオブジェクトに表示することができます。今回は以下の仕様を元に経路と目的値を各アンカーに記録しています。

* 目的地と途中経路の区別
* アンカーをつないだ時の1つ前のアンカーID（経路を明確にするため）
* 目的地のラベル名

##### 目的地と途中経路の区別
目的との区別は最終的にそこが目的地であることを明確にするためにUIを変更（ラベルを出す等）するためのフラグとして利用しています。例えば、このような方法で何か別のIDを紐づけると表示するコンテンツを色々と変更することができます。

##### アンカーをつないだ時の1つ前のアンカーID（経路を明確にするため）
今回は簡易的なデモなので、基本的には1本道で設定した順路に従うというものを用意しました。このアンカー同士をつないでいくことで道順をわかりやすくしています。
このような方法は経路探索を行う場合にも有効です。いわゆるアンカー同士で行き来ができる情報を保持することで、複数の経路を組合わせて色々な経路選択を実現することも可能になります。
今回のアプリでは、新しく途中経路/目的地のアンカーを設置する際に、その直前のアンカーIDを保持させることで、紐づけ関係を持たせています。

##### 目的地のラベル名
目的地には2つ要素があり、1.どのアンカーが終着点かわかるようにする、2.最初に行きたいところを選択するための候補として情報を記録しています。目的地は途中経路含めてルート上のすべてのアンカーに情報を記録します。
こうすることで、例えば「改札」を指定したときに、「改札」以外のアンカーをフィルターすることができ、経路の割り出しを容易にしています。

これらの方法はあくまで一例ですが、Azure Spatial Anchors＋経路に関する情報を格納することで道案内や、建物ないの導線コントロールが可能になります。

デモレベルであればこの方法でも容易に実現できますが、広域になればより効率的な手法が必要になると思います。
ぜひチャレンジしてみてください。

### アプリについて
Azure Spatial Anchorsはサービスの特性上、一度アップロードした情報は残り続けます。サンプルではアンカーの生存期間を7日で設定しています。永続化はしていません。
一度期限が過ぎた場合は基点のアンカー情報を削除してください。アンカー情報はアプリケーションのLocalState配下にある「SavedAzureAnchorID.txt」になります。
なお、今回はローカルストレージに基点になるIDを保存していますが、このIDを共有することで経路情報を他の人にも共有することが可能になります。

<div style="text-align: center;">
© 2020 Takahiro Miyaura All rights reserved.<br />
本コンテンツの著作権、および本コンテンツ中に出てくる商標権、団体名、ロゴ、製品、サービスなどはそれぞれ、各権利保有者に帰属します。
</div>
