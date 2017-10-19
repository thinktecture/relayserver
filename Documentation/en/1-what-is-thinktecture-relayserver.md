# [What is Thinktecture RelayServer?](1-what-is-thinktecture-relayserver.md)

The Thinktecture RelayServer (referred to as RelayServer in the following) provides open-source software for bi-directional, secure communication between clients, mobile devices and applications with on-premise applications behind routers and firewalls via the HTTPS protocol with complete centralized control and auditing of the allowed Participant and the permitted data.

A server that is equally available for client applications and on-premise applications is a secure exchange point for messages and data. Since the server is under the full control of the operator and communicates encrypted encrypted with both clients and on-premises applications, the security of the transmitted data is ensured at all times.

The positioning of the RelayServer in the public Internet also ensures that both clients and on-premise applications need only to establish outgoing connections to the communication. Thus, firewalls, routers and NAT systems are usually also a problem, as frequently changing IP addresses of simple DSL connections or mobile connections.

# Advantages of the RelayServer

- Open Source Software
- Full data integrity lies with the operator of the relay server
- Easy to implement as a pure software solution
- No libraries are required for the client software
- The solution is client-capable so that data can flow through a RelayServer for several different on-premise applications and their clients
- Firewalls, NAT and proxies are usually not a problem
- The transport channel is encrypted via HTTPS connection
- The On-Premises application requires an open outgoing HTTPS connection as a minimum requirement
- No special hardware required
- No firewall changes required
- No data retention in a DMZ is necessary

# Objective of the relay server

The requirements for enterprise applications have changed dramatically in recent years. The existing application environments consisting of servers and desktop-based clients in a local, secure network no longer address the current reality.

Companies and users demand concrete answers to these essential challenges from their solution providers:

## Challenge: Secure mobile data access
![1-challenge-mobile-data-access.png](./assets/1-challenge-mobile-data-access.png)
 
Mobile devices such as laptops, tablets and smartphones are an integral part of everyday business life. When accessing company data via such devices, the desire to no longer be bound to the local network is quickly created. The existing boundaries of the network must therefore be "transparent" without jeopardizing the security of company data.

The RelayServer supports the secure connection of mobile devices with On-Premises applications purely on the basis of software. The On-Premises Server can be connected to the Internet via a simple dynamic DSL uplink or a 3G connection.

## Challenge: Site networking
![1-site-networking.png](./assets/1-site-networking.png)

Today, companies do not only exist in one place, but they are spread over many locations in the form of outposts or branches. This local distribution results in the need for effective data networking, so that all sites can work together with the same company data.

The RelayServer ensures a fast and uncomplicated networking of the sites and seamlessly integrates itself into the application to be networked. Again, the On-Premises Server can be connected to the Internet via a simple dynamic DSL uplink or a 3G connection.

# Competing solutions for the RelayServer

## Open firewall ports
![1-firewall.png](./assets/1-firewall.png)

To access on-premises applications, a corresponding port can be opened in the corporate firewall. Accessing clients should then authorize the On-Premises application or the firewall via appropriate certificates to enable secure communication

Disadvantage:

1. Firewall must be accessible via a static I, an official DNS or a DynDNS entry.
1. On-Premises application or the firewall must support the authorization of accessing clients with certificates.
1. Certificates must be regularly updated and then distributed to the accessing devices of the clients, which means a high logistical effort.
1. Certificate management on devices increases support costs and makes it difficult for users to concentrate on their actual work.
1. The necessary firewall configurations can only be carried out by trained personnel.

## VPN
![1-vpn-gateway.png](./assets/1-vpn-gateway.png)

Virtual private networks (VPNs) generally allow secure communication between mobile clients and on-premises applications through the creation of secure virtual networks.

Disadvantage:

1. Before each On-Premises Server, a VPN gateway is required, with which the VPN clients can connect. These gateways often mean larger investments that impact the company's IT budget.
1. For the establishment and administration of VPN gateways, professionally trained personnel is necessary.
1. VPNs are often combined with the release of corresponding ports - as shown under "Open firewall ports". This creates an extremely complex and cost-intensive setup
1. The devices of the clients must support the establishment of VPN connections; a special VPN software must often be installed, configured and kept up-to-date.
1. The acceptance of VPNs by users is not particularly pronounced since the successful establishment of VPN connections is often experienced as problematic and complicated.

## Cloud Services
![1-cloud-service.png](./assets/1-cloud-service.png)

Cloud services enable clients and on-premise applications to be easily connected through the use of appropriate software libraries. But here, too, there are some disadvantages:

The use of the cloud as a relay point can be in conflict with the Federal Data Protection Act (BDSG), depending on the location of the cloud provider and target sector of the application. Encryption of message transport with SSL also does not help because in the cloud the previously protected SSL encryption has to be canceled for the processing of the messages and the messages are present at that moment in an encrypted form.

Cloud services often also offer a minimal set of features and do not consider the different requirements of companies. Individual adjustments are generally not planned.
