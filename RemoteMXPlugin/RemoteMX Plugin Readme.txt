
RemoteMXPluginSettings is parsed using DataContract serializer which requires that setting items are in certain order so DO NOT change the order of the items in SampleSettingsFile.xml!

RemoteMXPlugin requires modifications to app.config that uses this dll.

Add following serviceModel section to your app's config.

    <system.serviceModel>
        <bindings>
            <basicHttpBinding>
                <binding name="RdmSitesSOAPBinding" maxReceivedMessageSize="2147483647" />
            </basicHttpBinding>
        </bindings>
        <client>
            <endpoint address="[RemoteMX web service address here]"
                binding="basicHttpBinding" bindingConfiguration="RdmSitesSOAPBinding"
                contract="RemoteMXService.RdmSites" name="RdmSites" />
        </client>
    </system.serviceModel>


