using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;

namespace UmaMusume_RandomRoomMatch
{
	/// <summary>
	/// MainWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class MainWindow : Window
	{
		RaceData[] raceData; //全てのレースの情報

		List<RaceData>   sortList = new List<RaceData>();
		List<PlayerData> playerList = new List<PlayerData>();
		List<PlayerData> standbyList = new List<PlayerData>();

		public MainWindow()
		{
			InitializeComponent();

			//全てのレース情報を取得
			string appPath = System.AppDomain.CurrentDomain.BaseDirectory;
			string filename = "RaceDataList.csv";
			string filepath = appPath + "\\" + filename;
			raceData = ReadCSV(filepath);

			SortList();
		}

		/// <summary>
		/// レースのCSVを読み込み
		/// </summary>
		/// <param name="filePath">CSVのパス</param>
		/// <returns>取得したレースデータの配列</returns>
		private RaceData[] ReadCSV(string filePath)
		{
			RaceData[] raceData;
			string[] lines = File.ReadAllLines(filePath);

			raceData = new RaceData[lines.Length - 1];

			for (int i = 1; i < lines.Length; i++)
			{
				raceData[i - 1] = new RaceData(lines[i]);

				AddSortList(raceData[i - 1]);
			}

			return raceData;
		}

		/// <summary>
		/// ソートされた内容の追加
		/// </summary>
		/// <param name="data">レースデータ</param>
		private void AddSortList(RaceData data)
		{
			NarrowDownList.Items.Add(new string[] {
				data.id.ToString(),
				data.name,
				data.GetVenueName(),
				data.GetGroundName(),
				data.distance + "m",
				data.GetOptimumDistanceName(),
				data.GetAroundName()
			});
		}

		/// <summary>
		/// 条件に沿って絞り込み
		/// </summary>
		private void SortList()
		{
			//絞り込みされた内容を初期化
			sortList.Clear();

			//条件に合ったレースをリストに追加
			for(int i = 0; i < raceData.Length; i++)
			{
				RaceData data = raceData[i];
				if (CheckVenue(data) != true) continue;
				if (CheckGround(data) != true) continue;
				if (CheckOptimumDistance(data) != true) continue;
				if (CheckAround(data) != true) continue;

				sortList.Add(data);
			}

			//絞り込み内容を表示
			NarrowDownList.Items.Clear();
			for(int i = 0; i < sortList.Count; i++)
			{
				AddSortList(sortList[i]);
			}
		}

		/// <summary>
		/// レースの情報をテキスト化
		/// </summary>
		/// <param name="data">レースデータ</param>
		/// <returns>テキスト化されたレースデータ</returns>
		private string RaceInfoText(RaceData data)
		{
			return
				data.name + "\n" +
				data.GetVenueName() + " " + data.GetGroundName() + "\n" +
				data.distance + "m (" + data.GetOptimumDistanceName() + ") " + data.GetAroundName(); 
		}

		//絞り込みのチェック
		#region Check
		public bool? CheckVenue(RaceData data)
		{
			RaceData.VenueType type = data.venue;
			switch (type)
			{
				case RaceData.VenueType.Tokyo:
					return Venue_Tokyo.IsChecked;
				case RaceData.VenueType.Nakayama:
					return Venue_Nakayama.IsChecked;
				case RaceData.VenueType.Kyoto:
					return Venue_Kyoto.IsChecked;
				case RaceData.VenueType.Hanshin:
					return Venue_Hanshin.IsChecked;
				case RaceData.VenueType.TyuKyo:
					return Venue_TyuKyo.IsChecked;
				case RaceData.VenueType.Ooi:
					return Venue_Ooi.IsChecked;
				default:
					return false;
			}
		}

		public bool? CheckGround(RaceData data)
		{
			RaceData.GroundType type = data.ground;
			switch (type)
			{
				case RaceData.GroundType.Turf:
					return Ground_Turf.IsChecked;
				case RaceData.GroundType.Dirt:
					return Ground_Dirt.IsChecked;
				default:
					return false;
			}
		}

		public bool? CheckOptimumDistance(RaceData data)
		{
			RaceData.OptimumDistanceType type = data.optimumDistance;
			switch (type)
			{
				case RaceData.OptimumDistanceType.Sprint:
					return OptimumDistance_Sprint.IsChecked;
				case RaceData.OptimumDistanceType.Mylar:
					return OptimumDistance_Mylar.IsChecked;
				case RaceData.OptimumDistanceType.MiddleDistance:
					return OptimumDistance_MiddleDistance.IsChecked;
				case RaceData.OptimumDistanceType.Stayer:
					return OptimumDistance_Stayer.IsChecked;
				default:
					return false;
			}
		}

		public bool? CheckAround(RaceData data)
		{
			RaceData.AroundType type = data.around;
			switch (type)
			{
				case RaceData.AroundType.RightAround:
					return Around_Right.IsChecked;
				case RaceData.AroundType.LeftAround:
					return Around_Left.IsChecked;
				default:
					return false;
			}
		}

		#endregion

		//
		// イベントハンドラー
		//

		/// <summary>
		/// 絞り込みからランダムにレースを選択する
		/// </summary>
		private void RandomSelectClick(object sender, RoutedEventArgs e)
		{
			//何番目のデータを取得するか
			Random rand = new System.Random();
			int length = sortList.Count;

			int index = rand.Next(0, length);

			//選ばれたデータを表示
			BitmapImage bitmap = new BitmapImage();
			bitmap = new BitmapImage(new Uri("Images/" + sortList[index].name.ToString() + ".png", UriKind.Relative));
			Preview.Source = bitmap;

			RaceInfo.Text = RaceInfoText(sortList[index]);
		}

		/// <summary>
		/// 絞り込みの条件が変わったら絞り込みなおす
		/// </summary>
		private void CheckBoxClick(object sender, RoutedEventArgs e)
		{
			SortList();
		}

		/// <summary>
		/// リストを追加する
		/// </summary>
		private void AddList(object sender, RoutedEventArgs e)
		{
			if (CheckDuplicateName(playerList, InputName.Text)) return;

			PlayerData data = new PlayerData(InputName.Text);
			playerList.Add(data);
			ListUpdate();
		}

		/// <summary>
		/// リストを削除する
		/// </summary>
		private void RemoveList(object sender, RoutedEventArgs e)
		{
			int index = PlayerList.SelectedIndex;
			if (index == -1) return;

			playerList.RemoveAt(index);
			ListUpdate();
		}

		/// <summary>
		/// リストにプレイヤーを追加する
		/// </summary>
		private void AddPlayer(object sender, RoutedEventArgs e)
		{
			int index = PlayerList.SelectedIndex;
			if (index == -1) return;

			PlayerData data = playerList[index];

			if (CheckDuplicateName(standbyList, data.name)) return;

			standbyList.Add(data);
			ListUpdate();
		}

		/// <summary>
		/// リストからプレイヤーを削除する
		/// </summary>
		private void RemovePlayer(object sender, RoutedEventArgs e)
		{
			int index = StandbyPlayerList.SelectedIndex;
			if (index == -1) return;

			standbyList.RemoveAt(index);
			ListUpdate();
		}

		/// <summary>
		/// すべてのプレイヤーを追加する
		/// </summary>
		private void AddAllPlayer(object sender, RoutedEventArgs e)
		{
			standbyList = new List<PlayerData>(playerList);
			ListUpdate();
		}

		/// <summary>
		/// すべてプレイヤーを削除する
		/// </summary>
		private void RemoveAllPlayer(object sender, RoutedEventArgs e)
		{
			standbyList.Clear();
			ListUpdate();
		}

		/// <summary>
		/// 名前が重複してないかどうか
		/// </summary>
		bool CheckDuplicateName(List<PlayerData> list, string name)
		{
			for(int i = 0; i < list.Count; i++)
			{
				if (list[i].name == name)
					return true;
			}

			return false;
		}

		/// <summary>
		/// リストを更新する
		/// </summary>
		private void ListUpdate()
		{
			PlayerList.Items.Clear();
			StandbyPlayerList.Items.Clear();

			playerList.Sort((a, b) => string.Compare(a.name, b.name));
			standbyList.Sort((a, b) => string.Compare(a.name, b.name));

			for (int i = 0; i < playerList.Count; i++)
			{
				PlayerList.Items.Add(new string[] { playerList[i].name });
			}

			for (int i = 0; i < standbyList.Count; i++)
			{
				StandbyPlayerList.Items.Add(new string[] { standbyList[i].name });
			}
		}

		/// <summary>
		/// スコアが高い順に並べる
		/// </summary>
		private void PickUp(object sender, RoutedEventArgs e)
		{
			OrderPlayerList.Items.Clear();

			List<string[]> list = StandbyPlayerList.Items.Cast<string[]>().ToList();
			List<PlayerData> order = new List<PlayerData>();
			for(int i = 0; i < list.Count; i++)
			{
				order.Add(new PlayerData(list[i][0], list[i][1]));
			}

			order.Sort((a, b) => b.number - a.number);

			for(int i = 0; i < order.Count; i++)
			{
				OrderPlayerList.Items.Add(new string[] { i.ToString(), order[i].name ,order[i].number.ToString()});
			}
		}

		/// <summary>
		/// 表示をリセットする
		/// </summary>
		private void PickReset(object sender, RoutedEventArgs e)
		{
			OrderPlayerList.Items.Clear();
		}
	}
}

/// <summary>
/// プレイヤーのデータ
/// </summary>
public class PlayerData
{
	public string name; //名前
	public int number;  //絞り込みの数値

	public PlayerData(string name)
	{
		this.name = name;
		number = -1;
	}

	public PlayerData(string name, string number)
	{
		Random rand = new System.Random();

		this.name = name;
		bool b = int.TryParse(number, out this.number);

		if(b == false)
		{
			this.number = rand.Next(0, 101);
		}
	}
}

/// <summary>
/// レースのデータ
/// </summary>
public class RaceData
{
	/// <summary>
	/// 開催場所
	/// </summary>
	public enum VenueType
	{
		Error = -1,
		Tokyo,	
		Nakayama,
		Kyoto,
		Hanshin,
		TyuKyo,
		Ooi,
	}

	/// <summary>
	/// 地面の種類
	/// </summary>
	public enum GroundType
	{
		Error = -1,
		Turf,
		Dirt,
	}

	/// <summary>
	/// 距離
	/// </summary>
	public enum OptimumDistanceType
	{
		Error = -1,
		Sprint,
		Mylar,
		MiddleDistance,
		Stayer,
	}

	/// <summary>
	/// 旋回方向
	/// </summary>
	public enum AroundType
	{
		Error = -1,
		RightAround,
		LeftAround,
	}

	public int id; //ID
	public string name; //レース名
	public VenueType venue; //開催地
	public GroundType ground; //地面種類
	public int distance; //距離
	public OptimumDistanceType optimumDistance; //距離の種類
	public AroundType around; //旋回方向

	public RaceData(string str)
	{
		string[] data = str.Split(',');

		id = int.Parse(data[0]);
		name = data[1];
		venue = GetVenueEnum(data[2]);
		ground = GetGroundEnum(data[3]);
		distance = int.Parse(data[4].Remove(data[4].Length - 1));
		optimumDistance = GetOptimumDistanceEnum(data[5]);
		around = GetAroundEnum(data[6]);
	}

	VenueType GetVenueEnum(string data)
	{
		switch (data)
		{
			case "東京":
				return VenueType.Tokyo;
			case "中山":
				return VenueType.Nakayama;
			case "京都":
				return VenueType.Kyoto;
			case "阪神":
				return VenueType.Kyoto;
			case "中京":
				return VenueType.TyuKyo;
			case "大井":
				return VenueType.Ooi;
			default:
				return VenueType.Error;
		}
	}

	GroundType GetGroundEnum(string data)
	{
		switch (data)
		{
			case "芝":
				return GroundType.Turf;
			case "ダート":
				return GroundType.Dirt;
			default:
				return GroundType.Error;
		}
	}

	OptimumDistanceType GetOptimumDistanceEnum(string data)
	{
		switch (data)
		{
			case "短距離":
				return OptimumDistanceType.Sprint;
			case "マイル":
				return OptimumDistanceType.Mylar;
			case "中距離":
				return OptimumDistanceType.MiddleDistance;
			case "長距離":
				return OptimumDistanceType.Stayer;
			default:
				return OptimumDistanceType.Error;
		}
	}

	AroundType GetAroundEnum(string data)
	{
		switch (data)
		{
			case "右":
				return AroundType.RightAround;
			case "左":
				return AroundType.LeftAround;
			default:
				return AroundType.Error;
		}
	}

	public string GetVenueName()
	{
		switch (venue)
		{
			case VenueType.Tokyo:
				return "東京";
			case VenueType.Nakayama:
				return "中山";
			case VenueType.Kyoto:
				return "京都";
			case VenueType.Hanshin:
				return "阪神";
			case VenueType.TyuKyo:
				return "中京";
			case VenueType.Ooi:
				return "大井";
			default:
				return "Error";
		}
	}

	public string GetGroundName()
	{
		switch (ground)
		{
			case GroundType.Turf:
				return "芝";
			case GroundType.Dirt:
				return "ダート";
			default:
				return "Error";
		}
	}

	public string GetOptimumDistanceName()
	{
		switch (optimumDistance)
		{
			case OptimumDistanceType.Sprint:
				return "短距離";
			case OptimumDistanceType.Mylar:
				return "マイル";
			case OptimumDistanceType.MiddleDistance:
				return "中距離";
			case OptimumDistanceType.Stayer:
				return "長距離";
			default:
				return "Error";
		}
	}

	public string GetAroundName()
	{
		switch (around)
		{
			case AroundType.RightAround:
				return "右";
			case AroundType.LeftAround:
				return "左";
			default:
				return "Error";
		}
	}
}
