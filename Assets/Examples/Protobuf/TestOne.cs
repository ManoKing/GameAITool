using UnityEngine;
using System.Collections;
using System.IO;
//定义一个序列化与反序列化对象
[ProtoBuf.ProtoContract]
class Person
{
    [ProtoBuf.ProtoMember(1)]
    public string name;
    [ProtoBuf.ProtoMember(2)]
    public int age;
}
[ProtoBuf.ProtoContract]
class Dog
{
    [ProtoBuf.ProtoMember(1)]
    public string name;
    [ProtoBuf.ProtoMember(2)]
    public int age;
}
public class TestOne : MonoBehaviour
{
    void Start()
    {
        Person per = new Person();
        per.age = 1;
        per.name = "Mano";

        using (Stream s = File.OpenWrite("test.dat"))
        {
            //序列化对象到文件
            ProtoBuf.Serializer.Serialize<Person>(s, per);
        }

        Person per2 = null;
        using (Stream s = File.OpenRead("test.dat"))
        {
            //从文件中读取并反序列化到对象
            per2 = ProtoBuf.Serializer.Deserialize<Person>(s);

            //打印
            print("name>" + per2.name + " age>" + per2.age);
        }
        Dog dog = new Dog();
        dog.age = 24;
        dog.name = "Dog";
        byte[] personByte=PackCodec.Serialize(dog);
        Dog testPerson = PackCodec.Deserialize<Dog>(personByte);
        Debug.Log(testPerson.age+testPerson.name);
    }
    
}
