﻿using Dama.Data.Enums;

namespace Dama.Data.Models
{
    public class CategoryBuilder
    {
        private string name;
        private string description;
        private Color color;
        private int priority;
        private string userId;

        public CategoryBuilder CreateCategory(string name)
        {
            this.name = name;
            return this;
        }
        public CategoryBuilder WithDescription(string description)
        {
            this.description = description;
            return this;
        }
        public CategoryBuilder WithColor(Color color)
        {
            this.color = color;
            return this;
        }
        public CategoryBuilder WithPriority(int priority)
        {
            this.priority = priority;
            return this;
        }
        public CategoryBuilder WithUserId(string userId)
        {
            this.userId = userId;
            return this;
        }

        public static implicit operator Category(CategoryBuilder cb)
        {
            return new Category(cb.name, cb.description, cb.color, cb.priority, cb.userId);
        }
    }
}
