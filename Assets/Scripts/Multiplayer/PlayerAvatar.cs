using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Steamworks;

public class PlayerAvatar : MonoBehaviour
{
    public ulong _playerSteamId;
    private bool _avatarReceived;
    public RawImage _playerIcon;
    protected Callback<AvatarImageLoaded_t> ImageLoaded;


    private void Start()
    {
        ImageLoaded = Callback<AvatarImageLoaded_t>.Create(OnImageLoaded);
    }

    private void OnImageLoaded(AvatarImageLoaded_t callback)
    {
        if (callback.m_steamID.m_SteamID == _playerSteamId)
        {
            _playerIcon.texture = GetSteamImageAsTexture(callback.m_iImage);
        }
        else    //another player
        {
            return;
        }
    }

    public void PopulateUI()
    {
        if (!_avatarReceived) { GetPlayerIcon(); }
    }

    void GetPlayerIcon()
    {
        int imageId = SteamFriends.GetLargeFriendAvatar((CSteamID)_playerSteamId);

        if (imageId == -1) { return; }

        _playerIcon.texture = GetSteamImageAsTexture(imageId);
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