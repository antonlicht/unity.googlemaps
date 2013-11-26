
public abstract class RefCounted
{
    private int m_refCount = 0;

    public void AddRef()
    {
        m_refCount++;
    }

    public void Release()
    {
        m_refCount--;
        if (m_refCount == 0) Destroy();
    }

    public abstract void Destroy();

}
