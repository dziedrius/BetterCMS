﻿using BetterCms.Module.Root.ViewModels.Option;

namespace BetterCms.Module.Pages.ViewModels.Option
{
    public interface IOptionsContainer
    {
        System.Collections.Generic.IList<OptionViewModel> Options { get; set; }
    }
}