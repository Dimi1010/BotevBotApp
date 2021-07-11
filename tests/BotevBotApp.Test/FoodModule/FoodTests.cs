using BotevBotApp.FoodModule;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using Xunit;

namespace BotevBotApp.Test.FoodModule
{
    public class FoodTests
    {
        [Fact]
        public void TestSerialization()
        {
            // Arrange
            var expectedJsonString = @"{""Name"":""TestFoodSerial""}";
            var food = new Food { Name = "TestFoodSerial" };

            // Act
            var jsonString = JsonSerializer.Serialize(food);

            // Assert
            jsonString.Should().BeEquivalentTo(expectedJsonString);
        }

        [Fact]
        public void TestSerializationMultiple()
        {
            // Arrange
            var expectedJsonString = @"[{""Name"":""TestFoodSerialMultiple""}]";
            var foods = new List<Food> { new Food { Name = "TestFoodSerialMultiple" } };

            // Act
            var jsonString = JsonSerializer.Serialize(foods);

            // Assert
            jsonString.Should().BeEquivalentTo(expectedJsonString);
        }

        [Fact]
        public void TestDeserialization()
        {
            // Arrange
            var jsonString = @"{""Name"":""TestFoodDeserial""}";
            var expectedFood = new Food { Name = "TestFoodDeserial" };

            // Act
            var food = JsonSerializer.Deserialize<Food>(jsonString);

            // Assert
            food.Should().BeEquivalentTo(expectedFood);
        }
    }

    public class WeightedFoodTests
    {
        [Fact]
        public void TestSerialization()
        {
            // Arrange
            var expectedJsonString = @"{""SelectionWeight"":1,""Name"":""TestFoodSerial""}";
            var food = new WeightedFood { Name = "TestFoodSerial", SelectionWeight = 1 };

            // Act
            var jsonString = JsonSerializer.Serialize(food);

            // Assert
            jsonString.Should().BeEquivalentTo(expectedJsonString);
        }

        [Fact]
        public void TestDeserialization()
        {
            // Arrange
            var jsonString = @"{""Name"":""TestFoodDeserial"",""SelectionWeight"":1}";
            var expectedFood = new WeightedFood { Name = "TestFoodDeserial", SelectionWeight = 1 };

            // Act
            var food = JsonSerializer.Deserialize<WeightedFood>(jsonString);

            // Assert
            food.Should().BeEquivalentTo(expectedFood);
        }
    }
}
