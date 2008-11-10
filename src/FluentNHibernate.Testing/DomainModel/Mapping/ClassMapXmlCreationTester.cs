using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;
using NUnit.Framework;
using FluentNHibernate.Mapping;

namespace FluentNHibernate.Testing.DomainModel.Mapping
{
    [TestFixture]
    public class ClassMapXmlCreationTester
    {
        private XmlDocument document;

        private XmlElement elementForProperty(string propertyName)
        {
            string xpath = string.Format("class/property[@name='{0}']", propertyName);
            return (XmlElement) document.DocumentElement.SelectSingleNode(xpath);
        }

        [Test]
        public void BasicManyToManyMapping()
        {
        	new MappingTester<MappedObject>()
        		.ForMapping(map => map.HasManyToMany<ChildObject>(x => x.Children))
        		.Element("class/bag")
        			.HasAttribute("name", "Children")
					.DoesntHaveAttribute("cascade")
        		.Element("class/bag/key")
        			.HasAttribute("column", "MappedObject_id")
        		.Element("class/bag/many-to-many")
        			.HasAttribute("class", typeof (ChildObject).AssemblyQualifiedName)
        			.HasAttribute("column", "ChildObject_id")
					.DoesntHaveAttribute("fetch");
        }
        
        [Test]
        public void ManyToManyAsSet()
        {
            new MappingTester<MappedObject>()
                .ForMapping(m => m.HasManyToMany<ChildObject>(x => x.Children).AsSet())
                .Element("class/set")
                    .HasAttribute("name", "Children")
                    .HasAttribute("table", typeof(ChildObject).Name + "To" + typeof(MappedObject).Name)
                .Element("class/set/key")
                    .HasAttribute("column", "MappedObject_id")
                .Element("class/set/many-to-many")
                    .HasAttribute("class", typeof(ChildObject).AssemblyQualifiedName)
                    .HasAttribute("column", "ChildObject_id");
		}

		[Test]
		public void ManyToManyAsBag()
		{
            new MappingTester<MappedObject>()
                .ForMapping(m => m.HasManyToMany<ChildObject>(x => x.Children).AsBag())
                .Element("class/bag")
                    .HasAttribute("name", "Children")
                    .HasAttribute("table", typeof(ChildObject).Name + "To" + typeof(MappedObject).Name)
                .Element("class/bag/key")
                    .HasAttribute("column", "MappedObject_id")
                .Element("class/bag/many-to-many")
                    .HasAttribute("class", typeof(ChildObject).AssemblyQualifiedName)
                    .HasAttribute("column", "ChildObject_id");
		}
		
		[Test]
		public void ManyToManyAsSetWithChildForeignKey()
		{
            new MappingTester<MappedObject>()
                .ForMapping(m => m.HasManyToMany<ChildObject>(x => x.Children).AsSet().WithChildKeyColumn("TheKids_ID"))
                .Element("class/set")
                    .HasAttribute("name", "Children")
                    .HasAttribute("table", typeof(ChildObject).Name + "To" + typeof(MappedObject).Name)
                .Element("class/set/key")
                    .HasAttribute("column", "MappedObject_id")
                .Element("class/set/many-to-many")
                    .HasAttribute("class", typeof(ChildObject).AssemblyQualifiedName)
                    .HasAttribute("column", "TheKids_ID");
		}

		[Test]
		public void ManyToManyAsSetWithParentForeignKey()
		{
			new MappingTester<MappedObject>()
				.ForMapping(m => m.HasManyToMany<ChildObject>(x => x.Children).AsSet().WithParentKeyColumn("TheParentID"))
				.Element("class/set")
					.HasAttribute("name", "Children")
					.HasAttribute("table", typeof(ChildObject).Name + "To" + typeof(MappedObject).Name)
				.Element("class/set/key")
					.HasAttribute("column", "TheParentID")
				.Element("class/set/many-to-many")
					.HasAttribute("class", typeof(ChildObject).AssemblyQualifiedName)
					.HasAttribute("column", "ChildObject_id");
		}

		[Test]
		public void ManyToManyAsSetWithJoinFetchMode()
		{
            new MappingTester<MappedObject>()
                .ForMapping(m => m.HasManyToMany<ChildObject>(x => x.Children).AsSet().FetchType.Join())
                .Element("class/set")
                    .HasAttribute("name", "Children")
                    .HasAttribute("table", typeof(ChildObject).Name + "To" + typeof(MappedObject).Name)
                .Element("class/set/key")
                    .HasAttribute("column", "MappedObject_id")
                .Element("class/set/many-to-many")
                    .HasAttribute("class", typeof(ChildObject).AssemblyQualifiedName)
                    .HasAttribute("column", "ChildObject_id")
                    .HasAttribute("fetch", "join");
		}

		[Test]
        public void BasicOneToManyMapping()
        {
			new MappingTester<MappedObject>()
				.ForMapping(map => map.HasMany<ChildObject>(x => x.Children))
				.Element("class/bag")
					.HasAttribute("name", "Children")
				.Element("class/bag/key")
					.HasAttribute("column", "MappedObject_id")
				.Element("class/bag/one-to-many")
					.HasAttribute("class", typeof (ChildObject).AssemblyQualifiedName);
        }

        [Test]
        public void AdvancedOneToManyMapping()
        {
            var map = new ClassMap<MappedObject>();
            map.HasMany<ChildObject>(x => x.Children).LazyLoad().IsInverse();

            document = map.CreateMapping(new MappingVisitor());

            var element =
                (XmlElement)document.DocumentElement.SelectSingleNode("class/bag[@name='Children']");

            element.AttributeShouldEqual("lazy", "true");
            element.AttributeShouldEqual("inverse", "true");
        }

        [Test]
        public void AdvancedManyToManyMapping()
        {
            var map = new ClassMap<MappedObject>();
            map.HasManyToMany<ChildObject>(x => x.Children).LazyLoad().IsInverse();

            document = map.CreateMapping(new MappingVisitor());

            var element =
                (XmlElement)document.DocumentElement.SelectSingleNode("class/bag[@name='Children']");

            element.AttributeShouldEqual("lazy", "true");
            element.AttributeShouldEqual("inverse", "true");
        }

        [Test]
        public void BuildTheHeaderXmlWithAssemblyAndNamespace()
        {
            var map = new ClassMap<MappedObject>();
            document = map.CreateMapping(new MappingVisitor());

            document.DocumentElement.GetAttribute("assembly").ShouldEqual(typeof (MappedObject).Assembly.GetName().Name);
            document.DocumentElement.GetAttribute("namespace").ShouldEqual(typeof (MappedObject).Namespace);
        }

        [Test]
        public void CascadeAll_with_many_to_many()
        {
            var map = new ClassMap<MappedObject>();
            map.HasManyToMany<ChildObject>(x => x.Children).Cascade.All();

            document = map.CreateMapping(new MappingVisitor());
            var element = (XmlElement) document.DocumentElement.SelectSingleNode("class/bag[@name='Children']");

            element.AttributeShouldEqual("cascade", "all");
        }

        [Test]
        public void CascadeAll_with_one_to_many()
        {
        	new MappingTester<MappedObject>()
        		.ForMapping(map => map.HasMany<ChildObject>(x => x.Children).Cascade.All())
        		.Element("class/bag[@name='Children']").HasAttribute("cascade", "all");
        }

        [Test]
        public void Create_a_component_mapping()
        {
            var map = new ClassMap<MappedObject>();
            map.Component<ComponentOfMappedObject>(x => x.Component, c =>
                                                                         {
                                                                             c.Map(x => x.Name);
                                                                             c.Map(x => x.Age);
                                                                         });

            document = map.CreateMapping(new MappingVisitor());

            var componentElement =
                (XmlElement) document.DocumentElement.SelectSingleNode("class/component");

            componentElement.AttributeShouldEqual("name", "Component");
            componentElement.AttributeShouldEqual("insert", "true");
            componentElement.AttributeShouldEqual("update", "true");

            componentElement.ShouldHaveChild("property[@name='Name']");
            componentElement.ShouldHaveChild("property[@name='Age']");
        }

        [Test]
        public void CreateDiscriminator()
        {
            var map = new ClassMap<SecondMappedObject>();
            map.DiscriminateSubClassesOnColumn<string>("Type");

            document = map.CreateMapping(new MappingVisitor());
            var element = (XmlElement) document.DocumentElement.SelectSingleNode("class/discriminator");
            element.AttributeShouldEqual("column", "Type");
            element.AttributeShouldEqual("type", "String");
        }

        [Test]
        public void CreateTheSubClassMappings()
        {
            var map = new ClassMap<MappedObject>();

            map.DiscriminateSubClassesOnColumn<string>("Type")
                .SubClass<SecondMappedObject>().IsIdentifiedBy("red")
                .MapSubClassColumns(m => { m.Map(x => x.Name); });

            document = map.CreateMapping(new MappingVisitor());

            Debug.WriteLine(document.OuterXml);

            var element = (XmlElement) document.DocumentElement.SelectSingleNode("//subclass");
            element.AttributeShouldEqual("name", typeof(SecondMappedObject).AssemblyQualifiedName);
            element.AttributeShouldEqual("discriminator-value", "red");

            XmlElement propertyElement = element["property"];
            propertyElement.AttributeShouldEqual("column", "Name");
        }

    	[Test]
    	public void CreateDiscriminatorValueAtClassLevel()
		{
			var map = new ClassMap<MappedObject>();

			map.DiscriminateSubClassesOnColumn<string>("Type", "Foo")
				.SubClass<SecondMappedObject>().IsIdentifiedBy("Bar")
				.MapSubClassColumns(m => m.Map(x => x.Name));

			document = map.CreateMapping(new MappingVisitor());

			var element = (XmlElement)document.DocumentElement.SelectSingleNode("class");
			element.AttributeShouldEqual("discriminator-value", "Foo");
    	}

        [Test]
        public void Creating_a_many_to_one_reference()
        {
			new MappingTester<MappedObject>()
			    .ForMapping(map=>map.References(x => x.Parent))
			    .Element("class/many-to-one")
			        .HasAttribute("name", "Parent")
	                .HasAttribute("column", "Parent_id");
        }

    	[Test]
    	public void Many_to_one_reference_should_default_to_empty_cascade()
    	{
			new MappingTester<MappedObject>()
			    .ForMapping(map => map.References(x => x.Parent))
			    .Element("class/many-to-one")
				    .DoesntHaveAttribute("cascade");
    	}

        [Test]
        public void Creating_a_many_to_one_reference_sets_the_column_overrides()
        {
            var map = new ClassMap<MappedObject>();
            map.References(x => x.Parent).WithForeignKey();

            document = map.CreateMapping(new MappingVisitor());

            Debug.WriteLine(document.DocumentElement.OuterXml);

            var element = (XmlElement)document.DocumentElement.SelectSingleNode("class/many-to-one");

            element.AttributeShouldEqual("foreign-key", "FK_MappedObjectToParent");
        }

        [Test]
        public void Creating_a_one_to_one_reference()
        {
			new MappingTester<MappedObject>()
			    .ForMapping(map=>map.HasOne(x => x.Parent))
			    .Element("class/one-to-one")
                    .HasAttribute("name", "Parent")
                    .HasAttribute("class", typeof(SecondMappedObject).AssemblyQualifiedName);
        }

    	[Test]
    	public void One_to_one_reference_should_default_to_empty_cascade()
    	{
			new MappingTester<MappedObject>()
			    .ForMapping(map => map.HasOne(x => x.Parent))
			    .Element("class/one-to-one")
				    .DoesntHaveAttribute("cascade");
    	}

        [Test]
        public void Creating_a_one_to_one_reference_sets_the_column_overrides()
        {
            var map = new ClassMap<MappedObject>();
            map.HasOne(x => x.Parent).WithForeignKey();

            document = map.CreateMapping(new MappingVisitor());

            Debug.WriteLine(document.DocumentElement.OuterXml);

            var element = (XmlElement)document.DocumentElement.SelectSingleNode("class/one-to-one");

            element.AttributeShouldEqual("foreign-key", "FK_MappedObjectToParent");
        }

        [Test]
        public void DetermineTheTableName()
        {
            var map = new ClassMap<MappedObject>();
            map.TableName.ShouldEqual("[MappedObject]");

            map.WithTable("Different");
            map.TableName.ShouldEqual("Different");
        }

        [Test]
        public void CanSetTableName()
        {
            new MappingTester<MappedObject>()
                .ForMapping(m => m.WithTable("myTableName"))
                .Element("class").HasAttribute("table", "myTableName");
        }

        [Test]
        public void DomainClassMapAutomaticallyCreatesTheId()
        {
            var map = new ClassMap<MappedObject>();
            map.UseIdentityForKey(x => x.Id, "id");
            document = map.CreateMapping(new MappingVisitor());

            XmlElement idElement = document.DocumentElement["class"]["id"];
            idElement.ShouldNotBeNull();

            idElement.GetAttribute("name").ShouldEqual("Id");
            idElement.GetAttribute("column").ShouldEqual("id");
            idElement.GetAttribute("type").ShouldEqual("Int64");

            XmlElement generatorElement = idElement["generator"];
            generatorElement.ShouldNotBeNull();
            generatorElement.GetAttribute("class").ShouldEqual("identity");
        }

        [Test]
        public void Map_an_enumeration()
        {
            var map = new ClassMap<MappedObject>();
            map.Map(x => x.Color);

            document = map.CreateMapping(new MappingVisitor());
            XmlElement element = elementForProperty("Color");

            Debug.WriteLine(element.OuterXml);

            element.AttributeShouldEqual("type", typeof (GenericEnumMapper<ColorEnum>).AssemblyQualifiedName);
            element["column"].AttributeShouldEqual("name", "Color");
            element["column"].AttributeShouldEqual("sql-type", "string");
            element["column"].AttributeShouldEqual("length", "50");
        }

        [Test]
        public void MapASimplePropertyWithNoOverrides()
        {
            var map = new ClassMap<MappedObject>();
            map.Map(x => x.Name);

            document = map.CreateMapping(new MappingVisitor());
            XmlElement element = elementForProperty("Name");

            element.AttributeShouldEqual("name", "Name");
            element.AttributeShouldEqual("column", "Name");
            element.AttributeShouldEqual("type", "String");
        }

        [Test]
        public void WriteTheClassNode()
        {
            var map = new ClassMap<MappedObject>();
            document = map.CreateMapping(new MappingVisitor());

            XmlElement classElement = document.DocumentElement["class"];
            classElement.ShouldNotBeNull();

            classElement.AttributeShouldEqual("name", typeof (MappedObject).Name);
            classElement.AttributeShouldEqual("table", map.TableName);
        }

		[Test]
		public void DomainClassMapWithId()
		{
			var map = new ClassMap<MappedObject>();
			map.Id(x => x.Id, "id");
			document = map.CreateMapping(new MappingVisitor());

			XmlElement idElement = document.DocumentElement["class"]["id"];
			idElement.ShouldNotBeNull();

			idElement.GetAttribute("name").ShouldEqual("Id");
			idElement.GetAttribute("column").ShouldEqual("id");
			idElement.GetAttribute("type").ShouldEqual("Int64");

			XmlElement generatorElement = idElement["generator"];
			generatorElement.ShouldNotBeNull();
			generatorElement.GetAttribute("class").ShouldEqual("identity");
		}

		[Test]
		public void DomainClassMapWithIdNoColumn()
		{
			var map = new ClassMap<MappedObject>();
			map.Id(x => x.Id);
			document = map.CreateMapping(new MappingVisitor());

			XmlElement idElement = document.DocumentElement["class"]["id"];
			idElement.ShouldNotBeNull();

			idElement.GetAttribute("name").ShouldEqual("Id");
			idElement.GetAttribute("column").ShouldEqual("Id");
			idElement.GetAttribute("type").ShouldEqual("Int64");

			XmlElement generatorElement = idElement["generator"];
			generatorElement.ShouldNotBeNull();
			generatorElement.GetAttribute("class").ShouldEqual("identity");
		}

        [Test]
        public void ClassMapHasCorrectHBMFileName()
        {
            var expectedFileName = "MappedObject.hbm.xml";
            var map = new ClassMap<MappedObject>();
            Assert.AreEqual(expectedFileName , map.FileName);
        }

        [Test]
		public void DomainClassMapWithIdNoColumnAndGenerator()
		{
			var map = new ClassMap<MappedObject>();
			map.Id(x => x.Id).GeneratedBy.Native();
			document = map.CreateMapping(new MappingVisitor());

			XmlElement generatorElement = document.DocumentElement["class"]["id"]["generator"];

			generatorElement.ShouldNotBeNull();
			generatorElement.GetAttribute("class").ShouldEqual("native");
		}

   		[Test]
		public void Creating_a_many_to_one_reference_with_column_specified()
		{
   		    new MappingTester<MappedObject>()
   		        .ForMapping(m => m.References(x => x.Parent, "MyParentId"))
   		        .Element("class/many-to-one").HasAttribute("column", "MyParentId");
		}

        [Test]
        public void Creating_a_many_to_one_reference_with_column_specified_through_TheColumnNameIs_method()
        {
            new MappingTester<MappedObject>()
                .ForMapping(m => m.References(x => x.Parent).TheColumnNameIs("MyParentId"))
                .Element("class/many-to-one").HasAttribute("column", "MyParentId");
        }

		[Test]
		public void Creating_a_many_to_one_reference_using_specified_foreign_key()
		{
		    new MappingTester<MappedObject>()
		        .ForMapping(m => m.References(x => x.Parent).WithForeignKey("FK_MyForeignKey"))
		        .Element("class/many-to-one").HasAttribute("foreign-key", "FK_MyForeignKey");
		}

		[Test]
		public void Creating_a_many_to_one_reference_with_cascade_specified_as_None()
		{
		    new MappingTester<MappedObject>()
		        .ForMapping(m => m.References(x => x.Parent).Cascade.None())
		        .Element("class/many-to-one").HasAttribute("cascade", "none");
		}

		[Test]
		public void Creating_a_many_to_one_reference_with_fetchtype_set()
		{
			new MappingTester<MappedObject>()
				.ForMapping(m => m.References(x => x.Parent).FetchType.Select())
				.Element("class/many-to-one").HasAttribute("fetch", "select");
		}

        [Test]
        public void CanSetSchema()
        {
            new MappingTester<MappedObject>()
                .ForMapping(m => m.SchemaIs("test"))
                .HasAttribute("schema", "test");
        }

        [Test]
        public void CanSetAsUnique()
        {
            new MappingTester<MappedObject>()
                .ForMapping(m => m.Map(x => x.Name).WithUniqueConstraint())
                .Element("class/property").HasAttribute("unique", "true");
        }

        [Test]
        public void SpanningClassAcrossTwoTables()
        {
            new MappingTester<MappedObject>()
                .ForMapping(m => m.WithTable("tableTwo", t => t.Map(x => x.Name)))
                .Element("class/join").Exists();
        }
    }

    public class SecondMappedObject
    {
        public string Name { get; set; }
        public long Id { get; set; }
    }

    public class ComponentOfMappedObject
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }

    public enum ColorEnum
    {
        Blue,
        Green,
        Red
    }

    public class MappedObject
    {
        public ColorEnum Color { get; set; }

        public ComponentOfMappedObject Component { get; set; }

        public SecondMappedObject Parent { get; set; }

        public string Name { get; set; }

        public string NickName { get; set; }

        public IList<ChildObject> Children { get; set; }

        public long Id { get; set; }
    }

    public class ChildObject
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Position { get; set; }
    }
}
