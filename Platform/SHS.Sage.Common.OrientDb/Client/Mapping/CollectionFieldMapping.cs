﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Orient.Client.Mapping
{
    internal class ListNamedFieldMapping<TTarget> : CollectionNamedFieldMapping<TTarget>
    {
        private readonly Func<object> _listFactory;

        public ListNamedFieldMapping(PropertyInfo propertyInfo, string fieldPath) : base(propertyInfo, fieldPath)
        {
            _listFactory = FastConstructor.BuildConstructor(_propertyInfo.PropertyType);
        }

        protected override IList CreateListInstance(int collectionSize)
        {
            return (IList) _listFactory();
        }

        protected override void AddItemToList(IList list, int index, object item)
        {
            list.Add(item);
        }
    }

    internal class ArrayNamedFieldMapping<TTarget> : CollectionNamedFieldMapping<TTarget>
    {
        private Func<int, object> _arrayFactory;

        public ArrayNamedFieldMapping(PropertyInfo propertyInfo, string fieldPath)
            : base(propertyInfo, fieldPath)
        {
            _arrayFactory = FastConstructor.BuildConstructor<int>(propertyInfo.PropertyType);
        }


        protected override IList CreateListInstance(int collectionSize)
        {
            return (IList) _arrayFactory(collectionSize);
        }

        protected override void AddItemToList(IList list, int index, object item)
        {
            list[index] = item;
        }
    }


    internal abstract class CollectionNamedFieldMapping<TTarget> : NamedFieldMapping<TTarget>
    {
        private readonly TypeMapperBase _mapper;
        private readonly Type _targetElementType;
        private readonly bool _needsMapping;
        private Func<object> _elementFactory;

        public CollectionNamedFieldMapping(PropertyInfo propertyInfo, string fieldPath)
            : base(propertyInfo, fieldPath)
        {
            _targetElementType = GetTargetElementType();
            _needsMapping = !NeedsNoConversion(_targetElementType);
            if (_needsMapping)
            {
                _mapper = TypeMapperBase.GetInstanceFor(_targetElementType);
                _elementFactory = FastConstructor.BuildConstructor(_targetElementType);
            }
        }

        protected abstract IList CreateListInstance(int collectionSize);
        protected abstract void AddItemToList(IList list, int index, object item);

        protected override void MapToNamedField(ODocument document, TTarget typedObject)
        {
            object sourcePropertyValue = document.GetField<object>(_fieldPath);

            IList collection = sourcePropertyValue as IList;
            if (collection == null) // if we only have one item currently stored (but scope for more) we create a temporary list and put our single item in it.
            {
                collection = new ArrayList();
                if (sourcePropertyValue != null)
                    collection.Add(sourcePropertyValue);
            }

            // create instance of property type
            IList collectionInstance = CreateListInstance(collection.Count);

            for (int i = 0; i < collection.Count; i++)
            {
                var t = collection[i];
                object oMapped = t;
                if (_needsMapping)
                {
                    object element = _elementFactory();
                    _mapper.ToObject((ODocument) t, element);
                    oMapped = element;
                }

                AddItemToList(collectionInstance, i, oMapped);
            }

            SetPropertyValue(typedObject, collectionInstance);
        }

        private Type GetTargetElementType()
        {
            if (_propertyInfo.PropertyType.IsArray)
                return _propertyInfo.PropertyType.GetElementType();
            if (_propertyInfo.PropertyType.IsGenericType)
                return _propertyInfo.PropertyType.GetGenericArguments().First();

            throw new NotImplementedException();

        }

        private static bool NeedsNoConversion(Type elementType)
        {
            return elementType.IsPrimitive ||
                   (elementType == typeof (string)) ||
                   (elementType == typeof (DateTime)) ||
                   (elementType == typeof (decimal)) ||
                   (elementType == typeof (ORID));
        }

        public override void MapToDocument(TTarget typedObject, ODocument document)
        {
            var targetElementType = _needsMapping ? typeof (ODocument) : _targetElementType;
            var listType = typeof (List<>).MakeGenericType(targetElementType);
            var targetList = (IList) Activator.CreateInstance(listType);

            var sourceList = (IEnumerable) GetPropertyValue(typedObject);

            foreach (var item in sourceList)
                targetList.Add(_needsMapping ? _mapper.ToDocument(item) : item);

            document.SetField(_fieldPath, targetList);
        }
    }
}