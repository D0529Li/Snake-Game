﻿namespace Snake_Game
{
    [Serializable]
    public class Records
    {
        public Player RecordPlayer_Slow { get; set; } = new Player(SpeedOptions.Slow);
        public Player RecordPlayer_Medium { get; set; } = new Player(SpeedOptions.Medium);
        public Player RecordPlayer_Fast { get; set; } = new Player(SpeedOptions.Fast);

        public Records(Player player)
        {
            ModifyRecords(player);
        }

        // Need this for serialization
        public Records() { }

        public void ModifyRecords(Player player)
        {
            if (player.Speed == SpeedOptions.Slow && player.Score > RecordPlayer_Slow.Score)
            {
                RecordPlayer_Slow.Name = player.Name;
                RecordPlayer_Slow.Score = player.Score;
            }
            else if (player.Speed == SpeedOptions.Medium && player.Score > RecordPlayer_Medium.Score)
            {
                RecordPlayer_Medium.Name = player.Name;
                RecordPlayer_Medium.Score = player.Score;
            }
            else if (player.Speed == SpeedOptions.Fast && player.Score > RecordPlayer_Fast.Score)
            {
                RecordPlayer_Fast.Name = player.Name;
                RecordPlayer_Fast.Score = player.Score;
            }
        }
    }
}
