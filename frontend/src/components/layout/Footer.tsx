export default function Footer() {
  return (
    <footer className="border-t border-text-secondary/10 bg-bg-alt p-lg text-center">
      <p className="text-body-s text-text-secondary">
        © {new Date().getFullYear()} Plataforma Inteligente de Intercambio de Objetos — Ayacucho, Peru.
      </p>
    </footer>
  );
}
