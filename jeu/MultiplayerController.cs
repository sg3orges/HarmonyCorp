using Godot;
using System;
using System.Net.Security;
using System.Linq;

public partial class MultiplayerController : Control
{
	
	private const int port = 8910;
	
	private  const string address  = "10.3.140.12"; //adresse perso

	private ENetMultiplayerPeer peer;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Multiplayer.PeerConnected += PeerConnected;
		Multiplayer.PeerDisconnected += PeerDisconnected;
		Multiplayer.ConnectedToServer += ConnectedToServer;
		Multiplayer.ConnectionFailed += ConnectionFailed;
	}

	/// <summary>
	/// est lance quand la connexions a echoue , lance sur le client
	/// </summary>
	/// <exception cref="NotImplementedException"></exception>
	private void ConnectionFailed()
	{
		GD.Print("Connexion Failed");
	}

	/// <summary>
	/// est lance quand la connexion a marche, lance sur le client
	/// </summary>
	/// <exception cref="NotImplementedException"></exception>
	private void ConnectedToServer()
	{
		GD.Print("Connected To Server");
		RpcId(1,"sendPlayerInformation", GetNode<LineEdit>("LineEdit").Text, Multiplayer.GetUniqueId());
	}

	/// <summary>
	/// est lance quand le joueur Disconnect , est lance sur tout les peers
	/// </summary>
	/// <param name="id"> id du joueur deconnecte </param>
	/// <exception cref="NotImplementedException"></exception>
	private void PeerDisconnected(long id)
	{
		GD.Print("Player Disconnected:" + id.ToString());
		GameManager.Players.Remove(GameManager.Players.Where(i => i.Id == id ).First<PlayerInfo>());
		var players = GetTree().GetNodesInGroup("Player");
		foreach (var item  in players){
			if(item.Name == id.ToString()){
				item.QueueFree();
			}
		}

	}

	/// <summary>
	/// est lance quand le joueur est connecter , est lance sur tout les peers
	/// </summary>
	/// <param name="id"> id joueur connecter</param>
	/// <exception cref="NotImplementedException"></exception>
	private  void PeerConnected(long id){

		GD.Print("Player Connected !"+ id.ToString());
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void _on_host_button_down()
	{
		 peer = new ENetMultiplayerPeer();
		var error = peer.CreateServer(port, 2);
		if (error != Error.Ok){
			GD.Print("error cannot host! :"+ error.ToString());
			return;
		}
		peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);

		Multiplayer.MultiplayerPeer = peer;
		GD.Print("Wainting For Player!");
		sendPlayerInformation(GetNode<LineEdit>("LineEdit").Text,1);
		
	}

	public void _on_join_button_down(){

		peer = new ENetMultiplayerPeer();
		peer.CreateClient(address,port);
		peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);
		Multiplayer.MultiplayerPeer = peer;
		GD.Print("Joining Game!");
	}

	public void _on_stat_game_button_down(){
		Rpc("startGame");
	
		
	}
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void startGame(){
		foreach(var item in GameManager.Players){
			GD.Print(item.Name+ " is Playing");
		}
		var scene = ResourceLoader.Load<PackedScene>("res://TestScene.tscn").Instantiate<Node2D>();
		GetTree().Root.AddChild(scene);
		this.Hide();

	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
	private void sendPlayerInformation(string name, int id){
		PlayerInfo playerInfo = new PlayerInfo{
			Name = name,
			Id = id

		};
		if( !GameManager.Players.Contains(playerInfo)){
			GameManager.Players.Add(playerInfo);

		}

		if(Multiplayer.IsServer()){
			foreach(var item in GameManager.Players){

				Rpc("sendPlayerInformation",item.Name,item.Id);
			}
		}

		
	}

}


	




