#pragma  once

#include <vector>
#include <functional>

class Promise
{
	static std::vector<Promise*> promises;
	static void update();
public:

	Promise* executeAfter;
	void then(const std::function<void()> &callback) const
	{

	}
};

template<class T>
class PromiseImpl : public Promise
{
public:
	T callback;

};