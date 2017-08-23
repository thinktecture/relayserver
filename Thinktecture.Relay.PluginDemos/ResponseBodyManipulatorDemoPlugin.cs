﻿using System.Net.Http;
using System.Text;
using Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget;
using Thinktecture.Relay.Server.OnPremise;
using Thinktecture.Relay.Server.Plugins;

namespace Thinktecture.Relay.PluginDemos
{
	public class ResponseBodyManipulatorDemoPlugin : IResponseBodyManipulator
	{
		public byte[] HandleBody(IOnPremiseTargetResponse response, IOnPremiseConnectorRequest request,
			out HttpResponseMessage immidiateResponse)
		{
			immidiateResponse = null;

			if (response == null || request.Url.EndsWith("WhatIsTheAnswerToLiveTheUniverseAndEverything"))
			{
				return Encoding.UTF8.GetBytes(@"
MMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM
MMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM `MM'  VMMMMM
MMMMMV  MV   MMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM  VM  MMMMMM
MMMMMM  M  mMMMMMMMMMMMMMMMMMMMV''      '`VMMMMMMMMMMMMMM  MMMA `M  MM  MM
MM  VM  M  MMMMMMMMMMMMMMMV'                   'VMMMMMMMMM.  'MM  M  M'.MM
MM.M  M  MV  VMMMMMMMV'                         'VMMMMMMMMM.  '  V  V .MMM
MMA  V  M  M' ,MMMMMMV'                           'VMMMMMMMM.  ..     mMMM
MMMA `     V  MMMMMM'                               `VMMMMMMm  'S'   mMMMM
MMMM.,., AMMMMV                                      `VMMM'''   :   .MMMMM
MMMM  'B'   MMMMMV                                     M'      .'  .MMMMMM
MMMM: AV'  V                                          `    .mm.    MMMMMMM
MMMM.  `.                                               ..MMMMMm   MMMMMMM
MMMMM.. .  .mMMV.                                       .VMMMMMMA   VMMMMM
MMMMMM  AMMMMMM'  *         <^@^>        <==>        .*  'MMMMMMm    MMMMM
MMMMM'  MMMMMMV  .I                                 .a@.  V''MMMMA    MMMM
MMMMM   MMMMMM(a@:.                               .' @@!  .   'MMMm    MMM
MMMM'   MMMV'''  !@a :.                         .';.a@@R  ,             MM
MMMV    MV'   :  :@@@: :.                     .:  a@@@@!  ..............mM
MMM'          .  `@@@@ : `...             ..:' : a@@@@@'  MMMMMMMMMMMMMMMM
MMM..........   @@@@@a:      :'`:`------':  :  a@@@@@@@   MMMMMMMMMMMMMMMM
MMMMMMMMMMMMMMA   `@@@@@@@a:    :   ::   :  a@@@@@@@@@'  :MMMMMMMMMMMMMMMM
MMMMMMMMMMMMMMM.   `@@@@@@@@@@aaA. .;|. .Aaa@@@@@@@@@'  .AMMMMMMMMMMMMMMMM
MMMMMMMMMMMMMMMM.   `@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@    mMMMMMMMMMMMMMMMMM
MMMMMMMMMMMMMMMMM.    @@@@@@@@@@@'oOo.oOOo'@@@@@@@'    mMMMMMMMMMMMMMMMMMM
MMMMMMMMMMMMMMMMMMm    `@@@@@@@'OOOOOOxOOOOO'@@@V     mMMMMMMMMMMMMMMMMMMM
MMMMMMMMMMMMMMMMMMMA.     `@@@'OOOOOOOOxOOOOO'@'    .AMMMMMMMMMMMMMMMMMMMM
MMMMMMMMMMMMMMMMMMMMMA.     ''V@@AOOOOOOxOOOOO.   .AMMMMMMMMMMMMMMMMMMMMMM
MMMMMMMMMMMMMMMMMMMMMMMMm.       `OOOOOOOxXOOOo .mMMMMMMMMMMMMMMMMMMMMMMMM
MMMMMMMMMMMMMMMMMMMMMMMMMMMAm..   `OOOOOOoxOOOO: MMMMMMMMMMMMMMMMMMMMMMMMM
MMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMA`OOOOOOOxOOOO: MMMMMMMMMMMMMMMMMMMMMMMMM
MMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMA`OOOOOOxOOOO; MMMMMMMMMMMMMMMMMMMMMMMMM
MMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMA`OOOOOOOOO; AMMMMMMMMMMMMMMMMMMMMMMMMM
MMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMA`OOOOOO; AMMMMMMMMMMMMMMMMMMMMMMMMMMM
WIZMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMmmmmmmMMMMMMMMMMMMMMMMMMMMMMMMM* MJJ

88888b   d888b  88b  88 8 888888    88888b   888    88b  88 88  d888b  88
88   88 88   88 888b 88 P   88      88   88 88 88   888b 88 88 88   `  88
88   88 88   88 88`8b88     88      88888P 88   88  88`8b88 88 88      88
88   88 88   88 88 `888     88      88    d8888888b 88 `888 88 88   ,  `'
88888P   T888P  88  `88     88      88    88     8b 88  `88 88  T888P  88

(c) by Mike Jittlov
");

			}

			return null;
		}
	}
}
