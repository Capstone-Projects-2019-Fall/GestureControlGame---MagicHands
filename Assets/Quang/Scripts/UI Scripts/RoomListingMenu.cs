using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
public class RoomListingMenu : MonoBehaviourPunCallbacks
{
   [SerializeField]
   private Transform _content;
   [SerializeField]
   private RoomListing _roomListing;

   private List<RoomListing> _listings = new List<RoomListing>();
   
   private RoomsCanvases _roomCanvases;

   public void FirstInitialize(RoomsCanvases canvases)
   {
       _roomCanvases = canvases;
   }
   
   public override void OnJoinedRoom()
   {
       _roomCanvases.CurrentRoomCanvas.Show();
       _content.DestroyChildren();
       _listings.Clear();
   }

   public override void OnRoomListUpdate(List<Photon.Realtime.RoomInfo> roomList)
   {
        foreach (RoomInfo info in roomList)
        {
            if(info.RemovedFromList)
            {
                int index = _listings.FindIndex( x => x.RoomInfo.Name == info.Name);
                if (index != -1)
                {
                    Destroy(_listings[index].gameObject);
                    _listings.RemoveAt(index);
                }
            }
            else
            {   
                int index = _listings.FindIndex(x => x.RoomInfo.Name == info.Name);
                if (index == -1)
                {
                    RoomListing listing = (RoomListing)Instantiate(_roomListing, _content);
                    if (listing != null)
                    {
                        listing.SetRoomInfo(info);
                        _listings.Add(listing);
                    }
                }
                else
                {

                }
            }
        }
   }
}
