# SdpTransform
SDP parser and writer for .NET.
It transforms between SDP text, object, and JSON representations.

It's a useful library for C#/.NET developers working on VoIP or WebRTC apps.
Individual SDP fields or the entire SDP structure can be converted to text, object, or JSON representations.


The following code from DemoApp demonstrates how to convert SDP text, object, and JSON to each other.
```cs
        var sdpText = @"
                v=0
                o=mediasoup-client 10000 2 IN IP4 0.0.0.0
                s=-
                t=0 0
                a=ice-lite
                a=group:BUNDLE 0 1
                a=msid-semantic:WMS *
                a=fingerprint:sha-512 4E:14:FF:D2:8F:11:B7:D0:20:97:45:D4:4D:E0:D3:A0:92:BB:DC:92:69:7B:50:04:76:38:FC:BC:7F:A7:2D:92:01:D0:8F:0D:3F:67:4A:FB:3E:EC:BC:F2:BE:96:83:54:76:80:E5:6F:FA:21:26:12:55:A3:2F:01:7D:25:8D:0B
                m=audio 7 UDP/TLS/RTP/SAVPF 100
                c=IN IP4 127.0.0.1
                a=rtcp-mux
                a=rtcp-rsize
                a=sendonly
                a=mid:0
                a=msid:I4NgZL4G/PCejmmR 55dc6736-cb89-4c8a-9b14-9db41a1aaf5a
                a=ice-ufrag:hchivqia1vmdfg6o
                a=ice-pwd:f2nw1s0cnlkzp28q43w9299hoq5c639f
                a=ice-options:renomination
                a=setup:actpass
                a=candidate:udpcandidate 1 udp 1076302079 192.168.1.48 46186 typ host
                a=end-of-candidates
                a=ssrc:399475718 cname:I4NgZL4G/PCejmmR
                a=rtpmap:100 opus/48000/2
                a=fmtp:100 minptime=10;useinbandfec=1;sprop-stereo=1;usedtx=1
                a=extmap:1 urn:ietf:params:rtp-hdrext:sdes:mid 
                a=extmap:4 http://www.webrtc.org/experiments/rtp-hdrext/abs-send-time 
                a=extmap:10 urn:ietf:params:rtp-hdrext:ssrc-audio-level 
                m=video 7 UDP/TLS/RTP/SAVPF 101 102
                c=IN IP4 127.0.0.1
                a=rtcp-mux
                a=rtcp-rsize
                a=sendonly
                a=mid:1
                a=msid:I4NgZL4G/PCejmmR 78d5a305-0ccc-4672-8c41-b23f17fe290b
                a=ice-ufrag:hchivqia1vmdfg6o
                a=ice-pwd:f2nw1s0cnlkzp28q43w9299hoq5c639f
                a=ice-options:renomination
                a=setup:actpass
                a=candidate:udpcandidate 1 udp 1076302079 192.168.1.48 46186 typ host
                a=end-of-candidates
                a=ssrc:613668621 cname:I4NgZL4G/PCejmmR
                a=ssrc:613668622 cname:I4NgZL4G/PCejmmR
                a=ssrc-group:FID 613668621 613668622
                a=rtpmap:101 VP8/90000
                a=rtpmap:102 rtx/90000
                a=fmtp:102 apt=101
                a=rtcp-fb:101 transport-cc  
                a=rtcp-fb:101 ccm fir 
                a=rtcp-fb:101 nack  
                a=rtcp-fb:101 nack pli 
                a=extmap:1 urn:ietf:params:rtp-hdrext:sdes:mid 
                a=extmap:4 http://www.webrtc.org/experiments/rtp-hdrext/abs-send-time 
                a=extmap:5 http://www.ietf.org/id/draft-holmer-rmcat-transport-wide-cc-extensions-01 
                a=extmap:11 urn:3gpp:video-orientation 
                a=extmap:12 urn:ietf:params:rtp-hdrext:toffset 
                ";

        
        // Convert SDP text to object and back.
        var sdpObject = sdpText.ToSdp();
        var sdpTextFromObject = sdpObject.ToText();
        Console.WriteLine($"SDP OBJECT TO TEXT:{Environment.NewLine}{sdpTextFromObject}");

        // Convert SDP object to JSON, deserialize and back to text.
        var json = JsonSerializer.Serialize(sdpObject, new JsonSerializerOptions 
        { 
            // Ignore null values when serializing.
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull 
        });
        Console.WriteLine(json);
        Console.WriteLine();
        var sdpObjectFromJson = JsonSerializer.Deserialize<Sdp>(json);
        var sdpTextFromJson = sdpObjectFromJson.ToText();
        Console.WriteLine($"SDP JSON TO OBJECT:{Environment.NewLine}{sdpTextFromJson}");
```

When you run the above program (JSON data is displayed as a screnshot):
```
SDP OBJECT TO TEXT:
v=0
o=mediasoup-client 10000 2 IN IP4 0.0.0.0
s=-
t=0 0
a=ice-lite
a=group:BUNDLE 0 1
a=msid-semantic:WMS *
a=fingerprint:sha-512 4E:14:FF:D2:8F:11:B7:D0:20:97:45:D4:4D:E0:D3:A0:92:BB:DC:92:69:7B:50:04:76:38:FC:BC:7F:A7:2D:92:01:D0:8F:0D:3F:67:4A:FB:3E:EC:BC:F2:BE:96:83:54:76:80:E5:6F:FA:21:26:12:55:A3:2F:01:7D:25:8D:0B
m=audio 7 UDP/TLS/RTP/SAVPF 100
c=IN IP4 127.0.0.1
a=rtcp-mux
a=rtcp-rsize
a=sendonly
a=mid:0
a=msid:I4NgZL4G/PCejmmR 55dc6736-cb89-4c8a-9b14-9db41a1aaf5a
a=ice-ufrag:hchivqia1vmdfg6o
a=ice-pwd:f2nw1s0cnlkzp28q43w9299hoq5c639f
a=ice-options:renomination
a=setup:actpass
a=candidate:udpcandidate 1 udp 1076302079 192.168.1.48 46186 typ host
a=end-of-candidates
a=ssrc:399475718 cname:I4NgZL4G/PCejmmR
a=rtpmap:100 opus/48000/2
a=fmtp:100 minptime=10;useinbandfec=1;sprop-stereo=1;usedtx=1
a=extmap:1 urn:ietf:params:rtp-hdrext:sdes:mid
a=extmap:4 http://www.webrtc.org/experiments/rtp-hdrext/abs-send-time
a=extmap:10 urn:ietf:params:rtp-hdrext:ssrc-audio-level
m=video 7 UDP/TLS/RTP/SAVPF 101 102
c=IN IP4 127.0.0.1
a=rtcp-mux
a=rtcp-rsize
a=sendonly
a=mid:1
a=msid:I4NgZL4G/PCejmmR 78d5a305-0ccc-4672-8c41-b23f17fe290b
a=ice-ufrag:hchivqia1vmdfg6o
a=ice-pwd:f2nw1s0cnlkzp28q43w9299hoq5c639f
a=ice-options:renomination
a=setup:actpass
a=candidate:udpcandidate 1 udp 1076302079 192.168.1.48 46186 typ host
a=end-of-candidates
a=ssrc:613668621 cname:I4NgZL4G/PCejmmR
a=ssrc:613668622 cname:I4NgZL4G/PCejmmR
a=ssrc-group:FID 613668621 613668622
a=rtpmap:101 VP8/90000
a=rtpmap:102 rtx/90000
a=fmtp:102 apt=101
a=rtcp-fb:101 transport-cc
a=rtcp-fb:101 ccm fir
a=rtcp-fb:101 nack
a=rtcp-fb:101 nack pli
a=extmap:1 urn:ietf:params:rtp-hdrext:sdes:mid
a=extmap:4 http://www.webrtc.org/experiments/rtp-hdrext/abs-send-time
a=extmap:5 http://www.ietf.org/id/draft-holmer-rmcat-transport-wide-cc-extensions-01
a=extmap:11 urn:3gpp:video-orientation
a=extmap:12 urn:ietf:params:rtp-hdrext:toffset
```

`Screenshot of JSON data.`
![alt text](https://github.com/melihercan/Utilme/blob/master/doc/SdpJson.png)

```
SDP JSON TO OBJECT:
v=0
o=mediasoup-client 10000 2 IN IP4 0.0.0.0
s=-
t=0 0
a=ice-lite
a=group:BUNDLE 0 1
a=msid-semantic:WMS *
a=fingerprint:sha-512 4E:14:FF:D2:8F:11:B7:D0:20:97:45:D4:4D:E0:D3:A0:92:BB:DC:92:69:7B:50:04:76:38:FC:BC:7F:A7:2D:92:01:D0:8F:0D:3F:67:4A:FB:3E:EC:BC:F2:BE:96:83:54:76:80:E5:6F:FA:21:26:12:55:A3:2F:01:7D:25:8D:0B
m=audio 7 UDP/TLS/RTP/SAVPF 100
c=IN IP4 127.0.0.1
a=rtcp-mux
a=rtcp-rsize
a=sendonly
a=mid:0
a=msid:I4NgZL4G/PCejmmR 55dc6736-cb89-4c8a-9b14-9db41a1aaf5a
a=ice-ufrag:hchivqia1vmdfg6o
a=ice-pwd:f2nw1s0cnlkzp28q43w9299hoq5c639f
a=ice-options:renomination
a=setup:actpass
a=candidate:udpcandidate 1 udp 1076302079 192.168.1.48 46186 typ host
a=end-of-candidates
a=ssrc:399475718 cname:I4NgZL4G/PCejmmR
a=rtpmap:100 opus/48000/2
a=fmtp:100 minptime=10;useinbandfec=1;sprop-stereo=1;usedtx=1
a=extmap:1 urn:ietf:params:rtp-hdrext:sdes:mid
a=extmap:4 http://www.webrtc.org/experiments/rtp-hdrext/abs-send-time
a=extmap:10 urn:ietf:params:rtp-hdrext:ssrc-audio-level
m=video 7 UDP/TLS/RTP/SAVPF 101 102
c=IN IP4 127.0.0.1
a=rtcp-mux
a=rtcp-rsize
a=sendonly
a=mid:1
a=msid:I4NgZL4G/PCejmmR 78d5a305-0ccc-4672-8c41-b23f17fe290b
a=ice-ufrag:hchivqia1vmdfg6o
a=ice-pwd:f2nw1s0cnlkzp28q43w9299hoq5c639f
a=ice-options:renomination
a=setup:actpass
a=candidate:udpcandidate 1 udp 1076302079 192.168.1.48 46186 typ host
a=end-of-candidates
a=ssrc:613668621 cname:I4NgZL4G/PCejmmR
a=ssrc:613668622 cname:I4NgZL4G/PCejmmR
a=ssrc-group:FID 613668621 613668622
a=rtpmap:101 VP8/90000
a=rtpmap:102 rtx/90000
a=fmtp:102 apt=101
a=rtcp-fb:101 transport-cc
a=rtcp-fb:101 ccm fir
a=rtcp-fb:101 nack
a=rtcp-fb:101 nack pli
a=extmap:1 urn:ietf:params:rtp-hdrext:sdes:mid
a=extmap:4 http://www.webrtc.org/experiments/rtp-hdrext/abs-send-time
a=extmap:5 http://www.ietf.org/id/draft-holmer-rmcat-transport-wide-cc-extensions-01
a=extmap:11 urn:3gpp:video-orientation
a=extmap:12 urn:ietf:params:rtp-hdrext:toffset
```



