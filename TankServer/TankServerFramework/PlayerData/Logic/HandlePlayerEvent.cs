using System;

class HandlePlayerEvent {
    public void OnLogin(Player player) {
        Scene.instance.AddPlayer(player.id);
    }

    public void OnLogout(Player player) {
        Scene.instance.DelPlayer(player.id);
    }
}