
#include "stdafx.h"
#include "WidcommWrapper.h"
#include <cstdlib>


WMAPI void* __stdcall CreateWidcommStack()
{
		

	try{
	WidcommStackPPC* wdStack = new WidcommStackPPC;

	return wdStack;
	}catch(...){

		return NULL;
	}
}

WMAPI BOOL __stdcall DeleteWidcommStack(void* wdStack)
{
		
	try{
		 WaitForSingleObject(   stackMutex, INFINITE );	 
	WidcommStackPPC* pwdStack = (WidcommStackPPC*)wdStack;	
	
	delete pwdStack;	  

	ReleaseMutex( stackMutex );
	return true;
	}catch(...){
		ReleaseMutex( stackMutex );
		return false;
	}
}


WMAPI BOOL __stdcall IsStackServerUp(void* wdStack)
{
		 WaitForSingleObject(   stackMutex, INFINITE );	
  WidcommStackPPC* pwdStack = (WidcommStackPPC*)wdStack;
ReleaseMutex( stackMutex );
  //do not use this function to stay compatibel with older SDK versions !
  //return pwdStack->IsStackServerUp();

  return true;
}

WMAPI short __stdcall GetStackStatus(void* wdStack)
{
  WidcommStackPPC* pwdStack = (WidcommStackPPC*)wdStack;
  return pwdStack->bt_stack_status;
}

WMAPI BOOL __stdcall IsDeviceReady(void* wdStack)
{
  WidcommStackPPC* pwdStack = (WidcommStackPPC*)wdStack;

  //do not use this function to stay compatibel with older SDK versions !
  //return pwdStack->IsDeviceReady();

  return true;
}

WMAPI BOOL __stdcall SetAutoReconnect(void* wdStack)
{
  WidcommStackPPC* pwdStack = (WidcommStackPPC*)wdStack;

  pwdStack->SetAutoReconnect(true);

  return true;
}

WMAPI BOOL __stdcall StartInquiry(void* wdStack)
{
	WidcommStackPPC* pwdStack = (WidcommStackPPC*)wdStack;

  pwdStack->bt_counter = 0;

  int i = 0;

  while(i < 256)
  {
    memset(pwdStack->bt_devices[i],0,BLUETOOTH_MAX_NAME_SIZE);
    memset(pwdStack->bt_addresses[i],0,BD_ADDR_LEN);
    i++;
  }

	pwdStack->InquiryEventComplete = FALSE;

	return pwdStack->StartInquiry();
}

WMAPI void __stdcall StopInquiry(void* wdStack)
{
	WidcommStackPPC* pwdStack = (WidcommStackPPC*)wdStack;

  pwdStack->StopInquiry();
}

WMAPI BOOL __stdcall InquiryCompleteEvent(void* wdStack, int* device_index)
{
  WidcommStackPPC* pwdStack = (WidcommStackPPC*)wdStack;

  *device_index = pwdStack->bt_counter;

  return pwdStack->InquiryEventComplete;
}

WMAPI short __stdcall SppComPort(void* wdStack)
{	try{
			 WaitForSingleObject(   stackMutex, INFINITE );	
			 short result=-1;
  WidcommStackPPC* pwdStack = (WidcommStackPPC*)wdStack;
  //if (pwdStack!=NULL)
	//  result=pwdStack->comPort;
	ReleaseMutex( stackMutex );
  return result;
}catch(...){
	ReleaseMutex( stackMutex );
	return -1;
}
}

WMAPI int __stdcall SppRemoveConnection(void* wdStack)
{
	try{
			 WaitForSingleObject(   stackMutex, INFINITE );	
	WidcommStackPPC* pwdStack = (WidcommStackPPC*)wdStack;
	int result=pwdStack->RemoveConnection();
	ReleaseMutex( stackMutex );
	
	if (pwdStack!=NULL)
		return result;
	else
		return 10;
	}catch(...){
		ReleaseMutex( stackMutex );
		return 10;
	}
}

WMAPI int __stdcall Bond(void* wdStack,ULONGLONG p_bda,wchar_t* pin)
{
	WidcommStackPPC* pwdStack = (WidcommStackPPC*)wdStack;
		ULONGLONG address = (ULONGLONG)p_bda;

	BD_ADDR bda;

  bda[0] = ( address        & 0xff);
	bda[1] = ((address >>  8) & 0xff);
	bda[2] = ((address >> 16) & 0xff);
  bda[3] = ((address >> 24) & 0xff);
	bda[4] = ((address >> 32) & 0xff);
	bda[5] = ((address >> 40) & 0xff);

	WCHAR pp[4];
	pp[3]='1';
	pp[2]='2';
	pp[1]='3';
	pp[0]='4';
	return pwdStack->Bond(bda,pp);
}





WMAPI int __stdcall SppCreateConnection(void* wdStack, UINT8 scn, ULONGLONG p_bda)
{


	try{
				 WaitForSingleObject(   stackMutex, INFINITE );
	WidcommStackPPC* pwdStack = (WidcommStackPPC*)wdStack;

	ULONGLONG address = (ULONGLONG)p_bda;

	BD_ADDR bda;

  bda[0] = ( address        & 0xff);
	bda[1] = ((address >>  8) & 0xff);
	bda[2] = ((address >> 16) & 0xff);
  bda[3] = ((address >> 24) & 0xff);
	bda[4] = ((address >> 32) & 0xff);
	bda[5] = ((address >> 40) & 0xff);

	//pwdStack->OpenClient(
  //static TCHAR m_serviceNameForServer[BT_MAX_SERVICE_NAME_LEN + 1] = L"Bluetooth Serial Port";
  static TCHAR m_serviceNameForServerOld[BT_MAX_SERVICE_NAME_LEN + 1] = L"SPP";
  
  TCHAR        m_serviceName[BT_MAX_SERVICE_NAME_LEN + 1];

  // use compiled in service name for server
  memcpy(m_serviceName, m_serviceNameForServerOld, BT_MAX_SERVICE_NAME_LEN + 1);

  CSppClient::SPP_CLIENT_RETURN_CODE port_rc;
  port_rc = pwdStack->CreateConnection(bda, m_serviceName); 
  
	ReleaseMutex( stackMutex );

   if (CSppClient::SUCCESS == port_rc)
   {
	   
	 //return pwdStack->OpenClient(0,bda);
     return 1;
   }
   else if(CSppClient::NO_BT_SERVER == port_rc)
   {
     return 2;
   }
   else if(CSppClient::ALREADY_CONNECTED == port_rc)
   {
     return 3;
   }
   else if(CSppClient::NOT_CONNECTED == port_rc)
   {
     return 4;
   }
   else if(CSppClient::NOT_ENOUGH_MEMORY == port_rc)
   {
     return 5;
   }
   else if(CSppClient::INVALID_PARAMETER == port_rc)
   {
     return 6;
   }
   else if(CSppClient::UNKNOWN_ERROR == port_rc)
   {
     return 7;
   }

  return 0;
  }catch(...){
	  	ReleaseMutex( stackMutex );
		return 10;
	}

}

WMAPI wchar_t* __stdcall DeviceResponded(void* wdStack, ULONGLONG* adr, ULONGLONG device_index)
{
	WidcommStackPPC* pwdStack = (WidcommStackPPC*)wdStack;
 
  char buffer[6];
  wchar_t out[6];

  ULONGLONG btadr;
  ULONGLONG ul[6];
  
  ul[0] = (ULONGLONG)pwdStack->bt_addresses[device_index][0];
	ul[1] = (ULONGLONG)pwdStack->bt_addresses[device_index][1];
  ul[2] = (ULONGLONG)pwdStack->bt_addresses[device_index][2];
  ul[3] = (ULONGLONG)pwdStack->bt_addresses[device_index][3];
  ul[4] = (ULONGLONG)pwdStack->bt_addresses[device_index][4];
  ul[5] = (ULONGLONG)pwdStack->bt_addresses[device_index][5];

  btadr = ul[0];
  btadr = btadr | (ul[1] << 8);
  btadr = btadr | (ul[2] << 16);
  btadr = btadr | (ul[3] << 24);
  btadr = btadr | (ul[4] << 32);
  btadr = btadr | (ul[5] << 40);

  *adr = btadr;

  return pwdStack->bt_devices[device_index];
}