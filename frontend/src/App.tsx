import AppRouter from './routes/AppRouter';
import ToastContainer from './components/ui/Toast';

function App() {
  return (
    <>
      <a
        href="#main-content"
        className="sr-only focus:not-sr-only focus:fixed focus:left-md focus:top-md focus:z-50 focus:rounded-md focus:bg-primary focus:px-md focus:py-sm focus:text-white"
      >
        Saltar al contenido
      </a>
      <AppRouter />
      <ToastContainer />
    </>
  );
}

export default App;
