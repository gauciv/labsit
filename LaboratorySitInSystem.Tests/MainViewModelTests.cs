using LaboratorySitInSystem.ViewModels;
using Xunit;

namespace LaboratorySitInSystem.Tests
{
    public class MainViewModelTests
    {
        [Fact]
        public void CurrentView_InitiallyNull()
        {
            var vm = new MainViewModel();
            Assert.Null(vm.CurrentView);
        }

        [Fact]
        public void NavigateTo_SetsCurrentView()
        {
            var vm = new MainViewModel();
            var target = new StubViewModel();

            vm.NavigateTo(target);

            Assert.Same(target, vm.CurrentView);
        }

        [Fact]
        public void NavigateTo_RaisesPropertyChanged()
        {
            var vm = new MainViewModel();
            var target = new StubViewModel();
            string changedProperty = null;
            vm.PropertyChanged += (s, e) => changedProperty = e.PropertyName;

            vm.NavigateTo(target);

            Assert.Equal("CurrentView", changedProperty);
        }

        [Fact]
        public void NavigateTo_SwitchesBetweenViewModels()
        {
            var vm = new MainViewModel();
            var first = new StubViewModel();
            var second = new StubViewModel();

            vm.NavigateTo(first);
            Assert.Same(first, vm.CurrentView);

            vm.NavigateTo(second);
            Assert.Same(second, vm.CurrentView);
        }

        [Fact]
        public void Instance_ReturnsLastCreatedMainViewModel()
        {
            var vm = new MainViewModel();
            Assert.Same(vm, MainViewModel.Instance);
        }

        private class StubViewModel : ViewModelBase { }
    }
}
