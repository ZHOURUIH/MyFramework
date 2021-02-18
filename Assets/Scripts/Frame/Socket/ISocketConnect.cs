using System;

public interface ISocketConnect
{
	string getName();
	void update(float elapsedTime);
	void destroy();
}