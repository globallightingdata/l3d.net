namespace L3D.Net.Mapper
{
    public abstract class DtoMapperBase<T1, T2>
    {
        public T1 Convert(T2 element)
        {
            return element is null ? default : ConvertData(element);
        }

        public T2 Convert(T1 element)
        {
            return element is null ? default : ConvertData(element);
        }

        protected abstract T1 ConvertData(T2 element);

        protected abstract T2 ConvertData(T1 element);
    }
}
