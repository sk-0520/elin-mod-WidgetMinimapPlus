using Elin.Plugin.Generator.Test.Use.PluginInfoGeneratorUseTestSandbox;
using System;

namespace Elin.Plugin.Generator.Test.Use
{

    public class PluginInfoGeneratorUseTest
    {
        #region function

        [Fact]
        public void SimpleInstanceTest()
        {
            var configInstance = new SimpleConfig();
            Assert.IsType<SimpleConfig>(configInstance); // 当たり前だよなぁ！
        }

        [Fact]
        public void SimpleInstanceResetTest()
        {
            var configInstance = new SimpleConfig();
            Assert.Throws<NotSupportedException>(() => configInstance.Reset());
        }

        [Fact]
        public void SimpleBindTest()
        {
            var configInstance = new SimpleConfig();
            var configBound = SimpleConfig.Bind(new BepInEx.Configuration.ConfigFile("NUL", false), configInstance);
            // バインドを通すとプロキシになってる
            Assert.IsNotType<SimpleConfig>(configBound);
            // プロキシなので使用者側は何も考える必要はない, というか使用者は型を知っておく必要はない(どうしても高速化したい場合はそうでもないがこのテストでは関係ない)
            Assert.IsAssignableFrom<SimpleConfig>(configBound);
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(1, 2)]
        [InlineData(2, 3)]
        public void SimpleBindResetTest(int initValue, int changeValue)
        {
            Assert.NotEqual(initValue, changeValue); // ここで死ぬ場合テストが間違ってる

            var configInstance = new SimpleConfig()
            {
                Value = initValue,
            };
            var configBound = SimpleConfig.Bind(new BepInEx.Configuration.ConfigFile("NUL", false), configInstance);
            // 初期化で通した値になっている(本当は ConfigFile 経由で前回値が読まれるから正確ではないがこのテストではファイルがないのでOK)
            Assert.Equal(initValue, configBound.Value);

            // 値変更は直感的に使用可能
            configBound.Value = changeValue;
            Assert.Equal(changeValue, configBound.Value);

            // リセットしたら初期値に戻る
            configBound.Reset();
            Assert.NotEqual(changeValue, configBound.Value);
            Assert.Equal(initValue, configBound.Value);
        }

        [Fact]
        public void NestConfigTest()
        {
            var configInstance = new NestConfig()
            {
                ChildA = new ChildConfig()
                {
                    Data = 123,
                },
                ChildB = new ChildConfig()
                {
                    Data = 456,
                },
                Value = 789,
            };
            var configBound = NestConfig.Bind(new BepInEx.Configuration.ConfigFile("NUL", false), configInstance);

            // 値チェック自体にあまり意味はなく、重複したクラス(ChildConfig)があってもソースジェネレーターが重複除外して正しくプロキシクラスを生成させることが目的
            // そのため、このテストの実行よりコンパイルが通っていれば成功みたいなもの
            Assert.Equal(123, configBound.ChildA.Data);
            Assert.Equal(456, configBound.ChildB.Data);
            Assert.Equal(789, configBound.Value);
        }

        [Fact]
        public void SimpleCloneTest()
        {
            var configInstance = new SimpleConfig()
            {
                Value = 123,
            };

            var clonedConfig = configInstance.Clone();
            Assert.IsType<SimpleConfig>(clonedConfig);

            // 値は同じでインスタンスは違う
            Assert.Equal(clonedConfig.Value, configInstance.Value);
            Assert.NotSame(clonedConfig, configInstance);

            // クローンした後に元の値を変えてもクローンした方には影響しない
            configInstance.Value = 456;
            Assert.Equal(123, clonedConfig.Value);
        }

        [Fact]
        public void SimpleBindCloneTest()
        {
            var configInstance = new SimpleConfig()
            {
                Value = 123,
            };
            var configBound = SimpleConfig.Bind(new BepInEx.Configuration.ConfigFile("NUL", false), configInstance);

            var clonedConfig = configBound.Clone();
            // プロキシをクローンしても元のクラスのインスタンスが返ってくる
            Assert.IsType<SimpleConfig>(clonedConfig);

            // 値は同じでインスタンスは違う
            Assert.Equal(clonedConfig.Value, configBound.Value);
            Assert.NotSame(clonedConfig, configBound);

            // クローンした後に元の値を変えてもクローンした方には影響しない
            configBound.Value = 456;
            Assert.Equal(123, clonedConfig.Value);
        }

        [Fact]
        public void NestCloneTest()
        {
            var configInstance = new NestConfig()
            {
                ChildA = new ChildConfig()
                {
                    Data = 123,
                },
                ChildB = new ChildConfig()
                {
                    Data = 456,
                },
                Value = 789,
            };
            var configBound = NestConfig.Bind(new BepInEx.Configuration.ConfigFile("NUL", false), configInstance);

            var clonedConfigInstance = configInstance.Clone();
            var clonedConfigBound = configBound.Clone();

            Assert.IsType<NestConfig>(clonedConfigInstance);
            Assert.IsType<NestConfig>(clonedConfigBound);

            // 値は同じでインスタンスは違う
            Assert.Equal(clonedConfigInstance.Value, configInstance.Value);
            Assert.NotSame(clonedConfigInstance, configInstance);

            Assert.Equal(clonedConfigBound.Value, configBound.Value);
            Assert.NotSame(clonedConfigBound, configBound);

            Assert.Equal(clonedConfigInstance.ChildA.Data, configInstance.ChildA.Data);
            Assert.NotSame(clonedConfigInstance.ChildA, configInstance.ChildA);

            Assert.Equal(clonedConfigInstance.ChildB.Data, configInstance.ChildB.Data);
            Assert.NotSame(clonedConfigInstance.ChildB, configInstance.ChildB);
        }

        [Fact]
        public void SimpleCopyToTest()
        {
            var source = new SimpleConfig()
            {
                Value = 123,
            };
            var destination = new SimpleConfig()
            {
                Value = 999,
            };

            source.CopyTo(destination);

            Assert.Equal(123, destination.Value);
        }

        [Fact]
        public void NestCopyToTest()
        {
            var source = new NestConfig()
            {
                ChildA = new ChildConfig()
                {
                    Data = 123,
                    HiddenText = "A",
                },
                ChildB = new ChildConfig()
                {
                    Data = 456,
                    HiddenText = "B",
                },
                Value = 789,
                IgnoredValue = 111,
            };
            var destination = new NestConfig()
            {
                ChildA = new ChildConfig()
                {
                    Data = 1,
                    HiddenText = "X",
                },
                ChildB = new ChildConfig()
                {
                    Data = 2,
                    HiddenText = "Y",
                },
                Value = 3,
                IgnoredValue = 4,
            };

            source.CopyTo(destination);

            Assert.Equal(source.Value, destination.Value);
            Assert.NotEqual(source.IgnoredValue, destination.IgnoredValue);

            Assert.Equal(source.ChildA.Data, destination.ChildA.Data);
            Assert.NotEqual(source.ChildA.HiddenText, destination.ChildA.HiddenText);

            Assert.Equal(source.ChildB.Data, destination.ChildB.Data);
            Assert.NotEqual(source.ChildB.HiddenText, destination.ChildB.HiddenText);
        }

        [Fact]
        public void NestCopyToNullTest()
        {
            var source = new NestConfig()
            {
                ChildA = new ChildConfig()
                {
                    Data = 123,
                    HiddenText = "A",
                },
                ChildB = new ChildConfig()
                {
                    Data = 456,
                    HiddenText = "B",
                },
                Value = 789,
                IgnoredValue = 111,
            };
            var destination = new NestConfig()
            {
                ChildA = new ChildConfig()
                {
                    Data = 1,
                    HiddenText = "X",
                },
                ChildB = null!,
                Value = 3,
                IgnoredValue = 4,
            };

            var destinationChildA = destination.ChildA;

            source.CopyTo(destination);

            Assert.Equal(source.Value, destination.Value);
            Assert.NotEqual(source.IgnoredValue, destination.IgnoredValue);

            Assert.Same(destinationChildA, destination.ChildA);
            Assert.Equal(source.ChildA.Data, destination.ChildA.Data);
            Assert.NotEqual(source.ChildA.HiddenText, destination.ChildA.HiddenText);

            Assert.NotNull(destination.ChildB);
            Assert.Equal(source.ChildB.Data, destination.ChildB.Data);
            Assert.NotEqual(source.ChildB.HiddenText, destination.ChildB.HiddenText);
        }


        #endregion
    }
}
