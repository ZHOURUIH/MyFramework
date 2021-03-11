using System;

public interface IClassObject
{
	void resetProperty();
	void setDestroy(bool isDestroy);
	bool isDestroy();
	void setAssignID(ulong assignID);
	ulong getAssignID();
}