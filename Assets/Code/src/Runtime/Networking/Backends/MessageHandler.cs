﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HouraiTeahouse.FantasyCrescendo.Networking {

public class MessageHandlers {

  readonly Action<NetworkDataMessage>[] handlers;

  public MessageHandlers() {
    handlers = new Action<NetworkDataMessage>[byte.MaxValue];
  }

  public void RegisterHandler(byte code, Action<NetworkDataMessage> handler) {
    if (handler == null) return;
    handlers[code] += handler;
  }

  public void RegisterHandler<T>(byte code, Action<T> handler) where T : INetworkSerializable, new() {
    if (handler == null) return;
    RegisterHandler(code, dataMsg => {
      var message = dataMsg.ReadAs<T>();
      handler(message);
      (message as IDisposable)?.Dispose();
      ObjectPool<T>.Shared.Return(message);
    });
  }

  public void UnregisterHandler(byte code) => handlers[code] = null;

  internal void Execute(NetworkConnection connection, Deserializer deserializer) {
    if (deserializer.Size <= 0) return;
    byte header = deserializer.ReadByte();
    var handler = handlers[header];
    if (handler == null) return;
    handler(new NetworkDataMessage(connection, deserializer));
  }

}


}