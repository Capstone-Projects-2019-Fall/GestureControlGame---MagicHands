using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurrentRoomCanvas : MonoBehaviour
{
    private RoomsCanvases _roomCanvases;

    [SerializeField]
    private PlayerListingsMenu _playerListingMenu;
    [SerializeField]
    private LeaveRoomMenu _leaveRoomMenu;
    public LeaveRoomMenu LeaveRoomMenu {get {return _leaveRoomMenu;} }

    public void FirstInitialize(RoomsCanvases canvases)
    {
        _roomCanvases = canvases;
        _playerListingMenu.FirstInitialize(canvases);
        _leaveRoomMenu.FirstInitialize(canvases);
    }
    public void Show()
    {
        gameObject.SetActive(true);
    }
    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
