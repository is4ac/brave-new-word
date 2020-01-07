[System.Serializable]
class PlayerData
{
    public bool obstructionProductive;      // users must click on button to submit word
    public bool obstructionUnproductive;    // show currently selected word score
    public bool juiceProductive;            // feedback during highlighting of words
    public bool juiceUnproductive;          // word score is based on word rarity, not frequency?
    public string username;                 // the public username to display
    public bool instructions;               // whether or not to show the instructions
    public string userID;                   // the unique user ID
    public long myHighScore;                // the local high score of the player
    public int gameNumber;                  // the # of game the player is playing
    public float masterVolume;
    public float sfxVolume;
    public float musicVolume;

    public PlayerData(bool obP, bool obU, bool juiceP, bool juiceU,
                      string username, bool instructions, string userID,
                     long myHighScore, int gameNumber, float masterVolume,
                     float sfxVolume, float musicVolume)
    {
        obstructionProductive = obP;
        obstructionUnproductive = obU;
        juiceProductive = juiceP;
        juiceUnproductive = juiceU;
        this.username = username;
        this.instructions = instructions;
        this.userID = userID;
        this.myHighScore = myHighScore;
        this.gameNumber = gameNumber;
        this.masterVolume = masterVolume;
        this.sfxVolume = sfxVolume;
        this.musicVolume = musicVolume;
    }
}