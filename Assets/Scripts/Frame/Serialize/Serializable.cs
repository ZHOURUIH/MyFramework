using System;

public abstract class Serializable : ClassObject
{
	public abstract bool read(SerializerRead reader);
	public abstract void write(SerializerWrite writer);
}