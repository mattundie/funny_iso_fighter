using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Steamworks;

public class PlayerListItem : MonoBehaviour
{
    public string _playerName;
    public int _connectionId;
    public ulong _playerSteamId;
    public bool _avatarReceived;

    public TMP_Text PlayerNameText;
    public RawImage PlayerIcon;

    protected Callback<AvatarImageLoaded_t> ImageLoaded;

    private void Start()
    {
        ImageLoaded = Callback<AvatarImageLoaded_t>.Create(OnImageLoaded);
    }

    private void OnImageLoaded(AvatarImageLoaded_t callback)
    {
        if(callback.m_steamID.m_SteamID == _playerSteamId)
        {
            PlayerIcon.texture = GetSteamImageAsTexture(callback.m_iImage);
        }
        else    //another player
        {
            return;
        }
    }

    public void PopulateUI()
    {
        PlayerNameText.text = _playerName;
        if(!_avatarReceived) { GetPlayerIcon(); }
    }

    void GetPlayerIcon()
    {
        int imageId = SteamFriends.GetLargeFriendAvatar((CSteamID)_playerSteamId);

        if(imageId == -1) { return; }

        PlayerIcon.texture = GetSteamImageAsTexture(imageId);
    }

    private Texture2D GetSteamImageAsTexture(int iImage)
    {
        Texture2D texture = null;

        bool isValid = SteamUtils.GetImageSize(iImage, out uint width, out uint height);
        if (isValid)
        {
            byte[] image = new byte[width * height * 4];
            isValid = SteamUtils.GetImageRGBA(iImage, image, (int)(width * height * 4));
            if (isValid)
            {
                texture = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false, true);
                texture.LoadRawTextureData(image);
                texture.Apply();
            }
        }
        _avatarReceived = true;
        return texture;
    }
}
