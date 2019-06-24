#pragma once


namespace NetworkDirect
{

#ifndef ASSERT
#define ASSERT Assert
#if DBG
#define ASSERT_BENIGN( exp )    (void)(!(exp)?OutputDebugStringA("Assertion Failed:" #exp "\n"),FALSE:TRUE)
#else
#define ASSERT_BENIGN( exp )
#endif  // DBG
#endif  // ASSERT


//---------------------------------------------------------
// Lock wrapper.
//
class Lock
{
    CRITICAL_SECTION* m_pLock;

public:
    Lock( CRITICAL_SECTION* pLock ){ m_pLock = pLock; EnterCriticalSection( m_pLock ); }

    _Releases_lock_(* this->m_pLock)
    ~Lock(){ LeaveCriticalSection( m_pLock ); }
};
//---------------------------------------------------------


extern HANDLE ghHeap;

/*
inline void* __cdecl operator new(
    size_t count
    )
{
    return ::HeapAlloc( ghHeap, 0, count );
}


inline void __cdecl operator delete(
    void* object
    )
{
    if( object )
    {
        ::HeapFree( ghHeap, 0, object );
    }
}


inline void* __cdecl operator new[](
    size_t count
    )
{
    return ::HeapAlloc( ghHeap, 0, count );
}


inline void __cdecl operator delete[](
    void* object
    )
{
    if( object )
    {
        ::HeapFree( ghHeap, 0, object );
    }
}
*/
} // namespace NetworkDirect
