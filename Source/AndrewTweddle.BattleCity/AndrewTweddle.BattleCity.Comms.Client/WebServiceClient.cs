﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18052
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

[assembly: System.Runtime.Serialization.ContractNamespaceAttribute("http://challenge.entelect.co.za/", ClrNamespace="challenge.entelect.co.za")]



[System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
[System.ServiceModel.ServiceContractAttribute(Namespace="http://challenge.entelect.co.za/", ConfigurationName="Challenge")]
public interface Challenge
{
    
    // CODEGEN: Parameter 'return' requires additional schema information that cannot be captured using the parameter mode. The specific attribute is 'System.Xml.Serialization.XmlElementAttribute'.
    [System.ServiceModel.OperationContractAttribute(Action="http://challenge.entelect.co.za/Challenge/getStatusRequest", ReplyAction="http://challenge.entelect.co.za/Challenge/getStatusResponse")]
    [System.ServiceModel.XmlSerializerFormatAttribute()]
    [return: System.ServiceModel.MessageParameterAttribute(Name="return")]
    getStatusResponse getStatus(getStatusRequest request);
    
    [System.ServiceModel.OperationContractAttribute(Action="http://challenge.entelect.co.za/Challenge/getStatusRequest", ReplyAction="http://challenge.entelect.co.za/Challenge/getStatusResponse")]
    System.Threading.Tasks.Task<getStatusResponse> getStatusAsync(getStatusRequest request);
    
    // CODEGEN: Parameter 'return' requires additional schema information that cannot be captured using the parameter mode. The specific attribute is 'System.Xml.Serialization.XmlElementAttribute'.
    [System.ServiceModel.OperationContractAttribute(Action="http://challenge.entelect.co.za/Challenge/setActionRequest", ReplyAction="http://challenge.entelect.co.za/Challenge/setActionResponse")]
    [System.ServiceModel.FaultContractAttribute(typeof(challenge.entelect.co.za.EndOfGameException), Action="http://challenge.entelect.co.za/Challenge/setAction/Fault/EndOfGameException", Name="EndOfGameException")]
    [System.ServiceModel.XmlSerializerFormatAttribute()]
    [return: System.ServiceModel.MessageParameterAttribute(Name="return")]
    setActionResponse setAction(setActionRequest request);
    
    [System.ServiceModel.OperationContractAttribute(Action="http://challenge.entelect.co.za/Challenge/setActionRequest", ReplyAction="http://challenge.entelect.co.za/Challenge/setActionResponse")]
    System.Threading.Tasks.Task<setActionResponse> setActionAsync(setActionRequest request);
    
    // CODEGEN: Parameter 'return' requires additional schema information that cannot be captured using the parameter mode. The specific attribute is 'System.Xml.Serialization.XmlElementAttribute'.
    [System.ServiceModel.OperationContractAttribute(Action="http://challenge.entelect.co.za/Challenge/setActionsRequest", ReplyAction="http://challenge.entelect.co.za/Challenge/setActionsResponse")]
    [System.ServiceModel.FaultContractAttribute(typeof(challenge.entelect.co.za.EndOfGameException), Action="http://challenge.entelect.co.za/Challenge/setActions/Fault/EndOfGameException", Name="EndOfGameException")]
    [System.ServiceModel.XmlSerializerFormatAttribute()]
    [return: System.ServiceModel.MessageParameterAttribute(Name="return")]
    setActionsResponse setActions(setActionsRequest request);
    
    [System.ServiceModel.OperationContractAttribute(Action="http://challenge.entelect.co.za/Challenge/setActionsRequest", ReplyAction="http://challenge.entelect.co.za/Challenge/setActionsResponse")]
    System.Threading.Tasks.Task<setActionsResponse> setActionsAsync(setActionsRequest request);
    
    // CODEGEN: Parameter 'return' requires additional schema information that cannot be captured using the parameter mode. The specific attribute is 'System.Xml.Serialization.XmlArrayAttribute'.
    [System.ServiceModel.OperationContractAttribute(Action="http://challenge.entelect.co.za/Challenge/loginRequest", ReplyAction="http://challenge.entelect.co.za/Challenge/loginResponse")]
    [System.ServiceModel.FaultContractAttribute(typeof(challenge.entelect.co.za.EndOfGameException), Action="http://challenge.entelect.co.za/Challenge/login/Fault/EndOfGameException", Name="EndOfGameException")]
    [System.ServiceModel.FaultContractAttribute(typeof(challenge.entelect.co.za.NoBlameException), Action="http://challenge.entelect.co.za/Challenge/login/Fault/NoBlameException", Name="NoBlameException")]
    [System.ServiceModel.XmlSerializerFormatAttribute()]
    [return: System.ServiceModel.MessageParameterAttribute(Name="return")]
    loginResponse login(loginRequest request);
    
    [System.ServiceModel.OperationContractAttribute(Action="http://challenge.entelect.co.za/Challenge/loginRequest", ReplyAction="http://challenge.entelect.co.za/Challenge/loginResponse")]
    System.Threading.Tasks.Task<loginResponse> loginAsync(loginRequest request);
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("svcutil", "4.0.30319.17929")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://challenge.entelect.co.za/")]
public partial class game
{
    
    private int currentTickField;
    
    private events eventsField;
    
    private long millisecondsToNextTickField;
    
    private System.DateTime nextTickTimeField;
    
    private bool nextTickTimeFieldSpecified;
    
    private string playerNameField;
    
    private player[] playersField;
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified, Order=0)]
    public int currentTick
    {
        get
        {
            return this.currentTickField;
        }
        set
        {
            this.currentTickField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified, Order=1)]
    public events events
    {
        get
        {
            return this.eventsField;
        }
        set
        {
            this.eventsField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified, Order=2)]
    public long millisecondsToNextTick
    {
        get
        {
            return this.millisecondsToNextTickField;
        }
        set
        {
            this.millisecondsToNextTickField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified, Order=3)]
    public System.DateTime nextTickTime
    {
        get
        {
            return this.nextTickTimeField;
        }
        set
        {
            this.nextTickTimeField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()]
    public bool nextTickTimeSpecified
    {
        get
        {
            return this.nextTickTimeFieldSpecified;
        }
        set
        {
            this.nextTickTimeFieldSpecified = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified, Order=4)]
    public string playerName
    {
        get
        {
            return this.playerNameField;
        }
        set
        {
            this.playerNameField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("players", Form=System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable=true, Order=5)]
    public player[] players
    {
        get
        {
            return this.playersField;
        }
        set
        {
            this.playersField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("svcutil", "4.0.30319.17929")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://challenge.entelect.co.za/")]
public partial class events
{
    
    private blockEvent[] blockEventsField;
    
    private unitEvent[] unitEventsField;
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("blockEvents", Form=System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable=true, Order=0)]
    public blockEvent[] blockEvents
    {
        get
        {
            return this.blockEventsField;
        }
        set
        {
            this.blockEventsField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("unitEvents", Form=System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable=true, Order=1)]
    public unitEvent[] unitEvents
    {
        get
        {
            return this.unitEventsField;
        }
        set
        {
            this.unitEventsField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("svcutil", "4.0.30319.17929")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://challenge.entelect.co.za/")]
public partial class blockEvent
{
    
    private state newStateField;
    
    private bool newStateFieldSpecified;
    
    private point pointField;
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified, Order=0)]
    public state newState
    {
        get
        {
            return this.newStateField;
        }
        set
        {
            this.newStateField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()]
    public bool newStateSpecified
    {
        get
        {
            return this.newStateFieldSpecified;
        }
        set
        {
            this.newStateFieldSpecified = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified, Order=1)]
    public point point
    {
        get
        {
            return this.pointField;
        }
        set
        {
            this.pointField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("svcutil", "4.0.30319.17929")]
[System.SerializableAttribute()]
[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://challenge.entelect.co.za/")]
public enum state
{
    
    /// <remarks/>
    FULL,
    
    /// <remarks/>
    EMPTY,
    
    /// <remarks/>
    OUT_OF_BOUNDS,
    
    /// <remarks/>
    NONE,
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("svcutil", "4.0.30319.17929")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://challenge.entelect.co.za/")]
public partial class point
{
    
    private int xField;
    
    private int yField;
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified, Order=0)]
    public int x
    {
        get
        {
            return this.xField;
        }
        set
        {
            this.xField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified, Order=1)]
    public int y
    {
        get
        {
            return this.yField;
        }
        set
        {
            this.yField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("svcutil", "4.0.30319.17929")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://challenge.entelect.co.za/")]
public partial class delta
{
    
    private long millisecondsToNextTickField;
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified, Order=0)]
    public long millisecondsToNextTick
    {
        get
        {
            return this.millisecondsToNextTickField;
        }
        set
        {
            this.millisecondsToNextTickField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("svcutil", "4.0.30319.17929")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://challenge.entelect.co.za/")]
public partial class @base
{
    
    private int xField;
    
    private int yField;
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified, Order=0)]
    public int x
    {
        get
        {
            return this.xField;
        }
        set
        {
            this.xField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified, Order=1)]
    public int y
    {
        get
        {
            return this.yField;
        }
        set
        {
            this.yField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("svcutil", "4.0.30319.17929")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://challenge.entelect.co.za/")]
public partial class player
{
    
    private @base baseField;
    
    private bullet[] bulletsField;
    
    private string nameField;
    
    private unit[] unitsField;
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified, Order=0)]
    public @base @base
    {
        get
        {
            return this.baseField;
        }
        set
        {
            this.baseField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("bullets", Form=System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable=true, Order=1)]
    public bullet[] bullets
    {
        get
        {
            return this.bulletsField;
        }
        set
        {
            this.bulletsField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified, Order=2)]
    public string name
    {
        get
        {
            return this.nameField;
        }
        set
        {
            this.nameField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("units", Form=System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable=true, Order=3)]
    public unit[] units
    {
        get
        {
            return this.unitsField;
        }
        set
        {
            this.unitsField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("svcutil", "4.0.30319.17929")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://challenge.entelect.co.za/")]
public partial class bullet
{
    
    private direction directionField;
    
    private bool directionFieldSpecified;
    
    private int idField;
    
    private int xField;
    
    private int yField;
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified, Order=0)]
    public direction direction
    {
        get
        {
            return this.directionField;
        }
        set
        {
            this.directionField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()]
    public bool directionSpecified
    {
        get
        {
            return this.directionFieldSpecified;
        }
        set
        {
            this.directionFieldSpecified = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified, Order=1)]
    public int id
    {
        get
        {
            return this.idField;
        }
        set
        {
            this.idField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified, Order=2)]
    public int x
    {
        get
        {
            return this.xField;
        }
        set
        {
            this.xField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified, Order=3)]
    public int y
    {
        get
        {
            return this.yField;
        }
        set
        {
            this.yField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("svcutil", "4.0.30319.17929")]
[System.SerializableAttribute()]
[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://challenge.entelect.co.za/")]
public enum direction
{
    
    /// <remarks/>
    NONE,
    
    /// <remarks/>
    UP,
    
    /// <remarks/>
    DOWN,
    
    /// <remarks/>
    LEFT,
    
    /// <remarks/>
    RIGHT,
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("svcutil", "4.0.30319.17929")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://challenge.entelect.co.za/")]
public partial class unit
{
    
    private action actionField;
    
    private bool actionFieldSpecified;
    
    private direction directionField;
    
    private bool directionFieldSpecified;
    
    private int idField;
    
    private int xField;
    
    private int yField;
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified, Order=0)]
    public action action
    {
        get
        {
            return this.actionField;
        }
        set
        {
            this.actionField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()]
    public bool actionSpecified
    {
        get
        {
            return this.actionFieldSpecified;
        }
        set
        {
            this.actionFieldSpecified = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified, Order=1)]
    public direction direction
    {
        get
        {
            return this.directionField;
        }
        set
        {
            this.directionField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()]
    public bool directionSpecified
    {
        get
        {
            return this.directionFieldSpecified;
        }
        set
        {
            this.directionFieldSpecified = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified, Order=2)]
    public int id
    {
        get
        {
            return this.idField;
        }
        set
        {
            this.idField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified, Order=3)]
    public int x
    {
        get
        {
            return this.xField;
        }
        set
        {
            this.xField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified, Order=4)]
    public int y
    {
        get
        {
            return this.yField;
        }
        set
        {
            this.yField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("svcutil", "4.0.30319.17929")]
[System.SerializableAttribute()]
[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://challenge.entelect.co.za/")]
public enum action
{
    
    /// <remarks/>
    NONE,
    
    /// <remarks/>
    UP,
    
    /// <remarks/>
    DOWN,
    
    /// <remarks/>
    LEFT,
    
    /// <remarks/>
    RIGHT,
    
    /// <remarks/>
    FIRE,
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("svcutil", "4.0.30319.17929")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://challenge.entelect.co.za/")]
public partial class unitEvent
{
    
    private bullet bulletField;
    
    private int tickTimeField;
    
    private unit unitField;
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified, Order=0)]
    public bullet bullet
    {
        get
        {
            return this.bulletField;
        }
        set
        {
            this.bulletField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified, Order=1)]
    public int tickTime
    {
        get
        {
            return this.tickTimeField;
        }
        set
        {
            this.tickTimeField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified, Order=2)]
    public unit unit
    {
        get
        {
            return this.unitField;
        }
        set
        {
            this.unitField = value;
        }
    }
}

[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
[System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
[System.ServiceModel.MessageContractAttribute(WrapperName="getStatus", WrapperNamespace="http://challenge.entelect.co.za/", IsWrapped=true)]
public partial class getStatusRequest
{
    
    public getStatusRequest()
    {
    }
}

[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
[System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
[System.ServiceModel.MessageContractAttribute(WrapperName="getStatusResponse", WrapperNamespace="http://challenge.entelect.co.za/", IsWrapped=true)]
public partial class getStatusResponse
{
    
    [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://challenge.entelect.co.za/", Order=0)]
    [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public game @return;
    
    public getStatusResponse()
    {
    }
    
    public getStatusResponse(game @return)
    {
        this.@return = @return;
    }
}

[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
[System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
[System.ServiceModel.MessageContractAttribute(WrapperName="setAction", WrapperNamespace="http://challenge.entelect.co.za/", IsWrapped=true)]
public partial class setActionRequest
{
    
    [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://challenge.entelect.co.za/", Order=0)]
    [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public int arg0;
    
    [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://challenge.entelect.co.za/", Order=1)]
    [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public action arg1;
    
    public setActionRequest()
    {
    }
    
    public setActionRequest(int arg0, action arg1)
    {
        this.arg0 = arg0;
        this.arg1 = arg1;
    }
}

[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
[System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
[System.ServiceModel.MessageContractAttribute(WrapperName="setActionResponse", WrapperNamespace="http://challenge.entelect.co.za/", IsWrapped=true)]
public partial class setActionResponse
{
    
    [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://challenge.entelect.co.za/", Order=0)]
    [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public delta @return;
    
    public setActionResponse()
    {
    }
    
    public setActionResponse(delta @return)
    {
        this.@return = @return;
    }
}

[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
[System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
[System.ServiceModel.MessageContractAttribute(WrapperName="setActions", WrapperNamespace="http://challenge.entelect.co.za/", IsWrapped=true)]
public partial class setActionsRequest
{
    
    [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://challenge.entelect.co.za/", Order=0)]
    [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public action arg0;
    
    [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://challenge.entelect.co.za/", Order=1)]
    [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public action arg1;
    
    public setActionsRequest()
    {
    }
    
    public setActionsRequest(action arg0, action arg1)
    {
        this.arg0 = arg0;
        this.arg1 = arg1;
    }
}

[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
[System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
[System.ServiceModel.MessageContractAttribute(WrapperName="setActionsResponse", WrapperNamespace="http://challenge.entelect.co.za/", IsWrapped=true)]
public partial class setActionsResponse
{
    
    [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://challenge.entelect.co.za/", Order=0)]
    [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public delta @return;
    
    public setActionsResponse()
    {
    }
    
    public setActionsResponse(delta @return)
    {
        this.@return = @return;
    }
}

[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
[System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
[System.ServiceModel.MessageContractAttribute(WrapperName="login", WrapperNamespace="http://challenge.entelect.co.za/", IsWrapped=true)]
public partial class loginRequest
{
    
    public loginRequest()
    {
    }
}

[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
[System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
[System.ServiceModel.MessageContractAttribute(WrapperName="loginResponse", WrapperNamespace="http://challenge.entelect.co.za/", IsWrapped=true)]
public partial class loginResponse
{
    
    [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://challenge.entelect.co.za/", Order=0)]
    [System.Xml.Serialization.XmlArrayAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
    [System.Xml.Serialization.XmlArrayItemAttribute("states", Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
    [System.Xml.Serialization.XmlArrayItemAttribute("item", Form=System.Xml.Schema.XmlSchemaForm.Unqualified, NestingLevel=1)]
    public System.Nullable<state>[][] @return;
    
    public loginResponse()
    {
    }
    
    public loginResponse(System.Nullable<state>[][] @return)
    {
        this.@return = @return;
    }
}

[System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
public interface ChallengeChannel : Challenge, System.ServiceModel.IClientChannel
{
}

[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
public partial class ChallengeClient : System.ServiceModel.ClientBase<Challenge>, Challenge
{
    
    public ChallengeClient()
    {
    }
    
    public ChallengeClient(string endpointConfigurationName) : 
            base(endpointConfigurationName)
    {
    }
    
    public ChallengeClient(string endpointConfigurationName, string remoteAddress) : 
            base(endpointConfigurationName, remoteAddress)
    {
    }
    
    public ChallengeClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
            base(endpointConfigurationName, remoteAddress)
    {
    }
    
    public ChallengeClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
            base(binding, remoteAddress)
    {
    }
    
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    getStatusResponse Challenge.getStatus(getStatusRequest request)
    {
        return base.Channel.getStatus(request);
    }
    
    public game getStatus()
    {
        getStatusRequest inValue = new getStatusRequest();
        getStatusResponse retVal = ((Challenge)(this)).getStatus(inValue);
        return retVal.@return;
    }
    
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    System.Threading.Tasks.Task<getStatusResponse> Challenge.getStatusAsync(getStatusRequest request)
    {
        return base.Channel.getStatusAsync(request);
    }
    
    public System.Threading.Tasks.Task<getStatusResponse> getStatusAsync()
    {
        getStatusRequest inValue = new getStatusRequest();
        return ((Challenge)(this)).getStatusAsync(inValue);
    }
    
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    setActionResponse Challenge.setAction(setActionRequest request)
    {
        return base.Channel.setAction(request);
    }
    
    public delta setAction(int arg0, action arg1)
    {
        setActionRequest inValue = new setActionRequest();
        inValue.arg0 = arg0;
        inValue.arg1 = arg1;
        setActionResponse retVal = ((Challenge)(this)).setAction(inValue);
        return retVal.@return;
    }
    
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    System.Threading.Tasks.Task<setActionResponse> Challenge.setActionAsync(setActionRequest request)
    {
        return base.Channel.setActionAsync(request);
    }
    
    public System.Threading.Tasks.Task<setActionResponse> setActionAsync(int arg0, action arg1)
    {
        setActionRequest inValue = new setActionRequest();
        inValue.arg0 = arg0;
        inValue.arg1 = arg1;
        return ((Challenge)(this)).setActionAsync(inValue);
    }
    
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    setActionsResponse Challenge.setActions(setActionsRequest request)
    {
        return base.Channel.setActions(request);
    }
    
    public delta setActions(action arg0, action arg1)
    {
        setActionsRequest inValue = new setActionsRequest();
        inValue.arg0 = arg0;
        inValue.arg1 = arg1;
        setActionsResponse retVal = ((Challenge)(this)).setActions(inValue);
        return retVal.@return;
    }
    
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    System.Threading.Tasks.Task<setActionsResponse> Challenge.setActionsAsync(setActionsRequest request)
    {
        return base.Channel.setActionsAsync(request);
    }
    
    public System.Threading.Tasks.Task<setActionsResponse> setActionsAsync(action arg0, action arg1)
    {
        setActionsRequest inValue = new setActionsRequest();
        inValue.arg0 = arg0;
        inValue.arg1 = arg1;
        return ((Challenge)(this)).setActionsAsync(inValue);
    }
    
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    loginResponse Challenge.login(loginRequest request)
    {
        return base.Channel.login(request);
    }
    
    public System.Nullable<state>[][] login()
    {
        loginRequest inValue = new loginRequest();
        loginResponse retVal = ((Challenge)(this)).login(inValue);
        return retVal.@return;
    }
    
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    System.Threading.Tasks.Task<loginResponse> Challenge.loginAsync(loginRequest request)
    {
        return base.Channel.loginAsync(request);
    }
    
    public System.Threading.Tasks.Task<loginResponse> loginAsync()
    {
        loginRequest inValue = new loginRequest();
        return ((Challenge)(this)).loginAsync(inValue);
    }
}
namespace challenge.entelect.co.za
{
    
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Xml.Serialization.XmlSchemaProviderAttribute("ExportSchema")]
    [System.Xml.Serialization.XmlRootAttribute(IsNullable=false)]
    public partial class EndOfGameException : object, System.Xml.Serialization.IXmlSerializable
    {
        
        private System.Xml.XmlNode[] nodesField;
        
        private static System.Xml.XmlQualifiedName typeName = new System.Xml.XmlQualifiedName("EndOfGameException", "http://challenge.entelect.co.za/");
        
        public System.Xml.XmlNode[] Nodes
        {
            get
            {
                return this.nodesField;
            }
            set
            {
                this.nodesField = value;
            }
        }
        
        public void ReadXml(System.Xml.XmlReader reader)
        {
            this.nodesField = System.Runtime.Serialization.XmlSerializableServices.ReadNodes(reader);
        }
        
        public void WriteXml(System.Xml.XmlWriter writer)
        {
            System.Runtime.Serialization.XmlSerializableServices.WriteNodes(writer, this.Nodes);
        }
        
        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }
        
        public static System.Xml.XmlQualifiedName ExportSchema(System.Xml.Schema.XmlSchemaSet schemas)
        {
            System.Runtime.Serialization.XmlSerializableServices.AddDefaultSchema(schemas, typeName);
            return typeName;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Xml.Serialization.XmlSchemaProviderAttribute("ExportSchema")]
    [System.Xml.Serialization.XmlRootAttribute(IsNullable=false)]
    public partial class NoBlameException : object, System.Xml.Serialization.IXmlSerializable
    {
        
        private System.Xml.XmlNode[] nodesField;
        
        private static System.Xml.XmlQualifiedName typeName = new System.Xml.XmlQualifiedName("NoBlameException", "http://challenge.entelect.co.za/");
        
        public System.Xml.XmlNode[] Nodes
        {
            get
            {
                return this.nodesField;
            }
            set
            {
                this.nodesField = value;
            }
        }
        
        public void ReadXml(System.Xml.XmlReader reader)
        {
            this.nodesField = System.Runtime.Serialization.XmlSerializableServices.ReadNodes(reader);
        }
        
        public void WriteXml(System.Xml.XmlWriter writer)
        {
            System.Runtime.Serialization.XmlSerializableServices.WriteNodes(writer, this.Nodes);
        }
        
        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }
        
        public static System.Xml.XmlQualifiedName ExportSchema(System.Xml.Schema.XmlSchemaSet schemas)
        {
            System.Runtime.Serialization.XmlSerializableServices.AddDefaultSchema(schemas, typeName);
            return typeName;
        }
    }
}
