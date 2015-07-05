#include "PushBullet.h"
#include "json.h"
#include <curl/curl.h>
#include <cstddef>
#include <sstream>
#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>
#include <string.h>


#ifdef USE_OPENSSL
#include <openssl/crypto.h>
static void lock_callback(int mode, int type, char *file, int line)
{
	(void)file;
	(void)line;
	if (mode & CRYPTO_LOCK) {
		pthread_mutex_lock(&(lockarray[type]));
	}
	else {
		pthread_mutex_unlock(&(lockarray[type]));
	}
}

static unsigned long thread_id(void)
{
	unsigned long ret;

	ret = (unsigned long)pthread_self();
	return(ret);
}

static void init_locks(void)
{
	int i;

	lockarray = (pthread_mutex_t *)OPENSSL_malloc(CRYPTO_num_locks() *
		sizeof(pthread_mutex_t));
	for (i = 0; i < CRYPTO_num_locks(); i++) {
		pthread_mutex_init(&(lockarray[i]), NULL);
	}

	CRYPTO_set_id_callback((unsigned long(*)())thread_id);
	CRYPTO_set_locking_callback((void(*)())lock_callback);
}

static void kill_locks(void)
{
	int i;

	CRYPTO_set_locking_callback(NULL);
	for (i = 0; i < CRYPTO_num_locks(); i++)
		pthread_mutex_destroy(&(lockarray[i]));

	OPENSSL_free(lockarray);
}
#endif


PushBullet::PushBullet(const json::Value& config)
{
	key = config["pushbullet"]["token"].asString();
	running = true;
	thread = std::thread(std::bind(&PushBullet::threadFunc, this));
}

PushBullet::~PushBullet()
{
	running = false;
	thread.join();
}


void PushBullet::update()
{
	mutex.lock();
	for (int i = 0; i < (int)jobs.size(); i++)
	{
		if (jobs[i]->done)
		{
			mutex.unlock();
			if (jobs[i]->callback)
				jobs[i]->callback(json::readJson(jobs[i]->result));
			mutex.lock();
			delete jobs[i];
			jobs.erase(jobs.begin() + i);
			i--;
		}
	}
	mutex.unlock();

}

size_t write_callback(char *ptr, size_t size, size_t nmemb, void *userdata)
{
	std::string* data = (std::string*)userdata;
	*data += std::string(ptr, size*nmemb);
	return size * nmemb;
}

void PushBullet::threadFunc()
{
	CURL *curl;
	curl_global_init(CURL_GLOBAL_DEFAULT);

	while (running)
	{
		mutex.lock();
		std::vector<Job*> jobsCopy = jobs;
		mutex.unlock();

		for (auto j : jobsCopy)
		{
			if (!j->done)
			{
				CURLcode res;
				curl = curl_easy_init();
				std::string response;
				struct curl_slist *chunk = NULL;
				chunk = curl_slist_append(chunk, ("Authorization: Bearer " + key).c_str());
				chunk = curl_slist_append(chunk, "Content-Type: application/json");
				curl_easy_setopt(curl, CURLOPT_URL, j->url.c_str());
				curl_easy_setopt(curl, CURLOPT_HTTPHEADER, chunk);
				curl_easy_setopt(curl, CURLOPT_NOPROGRESS, 1L);
				curl_easy_setopt(curl, CURLOPT_SSL_VERIFYPEER, 0L);
				curl_easy_setopt(curl, CURLOPT_SSL_VERIFYHOST, 0L);
				curl_easy_setopt(curl, CURLOPT_WRITEFUNCTION, write_callback);
				curl_easy_setopt(curl, CURLOPT_WRITEDATA, &response);
				if (j->method == "POST" && j->data != "")
				{
					//curl_easy_setopt(curl, CURLOPT_CUSTOMREQUEST, "POST");
					curl_easy_setopt(curl, CURLOPT_POSTFIELDS, j->data.c_str());
					//curl_easy_setopt(curl, CURLOPT_POSTFIELDSIZE_LARGE, (curl_off_t)j->data.size());
				}

				res = curl_easy_perform(curl);
				if (res != CURLE_OK)
					fprintf(stderr, "curl_easy_perform() failed: %s\n",
					curl_easy_strerror(res));
				curl_easy_cleanup(curl);

				printf("%s\n", response.c_str());
				j->result = response;
				if (j->result == "")
				{
					printf("PushBullet: no response! for %s\n", j->url.c_str());
					j->result = "{}";
				}
				j->done = true;
			}
		}
		usleep(100);
	}


	curl_global_cleanup();
}



const PushBullet::Job& PushBullet::getDevices(std::function<void(const std::vector<std::string> &)> callback)
{
	Job* job = new Job();

	job->url = "https://api.pushbullet.com/v2/devices";
	job->method = "GET";
	job->callback = [callback](const json::Value& data)
	{
		printf("Callback1\n");
		std::vector<std::string> devices;

		//for (auto d : data["devices"])
		{
			//printf(data)
		}


		callback(devices);
	};

	mutex.lock();
	jobs.push_back(job);
	mutex.unlock();


	return *job;
}

const PushBullet::Job& PushBullet::sendMessage(const std::string &title, const std::string &message, const std::function<void(const json::Value&)> &callback)
{
	Job* job = new Job();

	job->url = "https://api.pushbullet.com/v2/pushes";
	job->method = "POST";

	json::Value packet;
	packet["type"] = "note";
	packet["title"] = title;
	packet["body"] = message;

	job->data =  ((std::stringstream&)(std::stringstream()<<packet)).str();
	job->callback = [callback](const json::Value& data)
	{
		printf("Callback pushes\n");
	};

	mutex.lock();
	jobs.push_back(job);
	mutex.unlock();


	return *job;

}
