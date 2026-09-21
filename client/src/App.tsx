import { AuthorBooksPage } from "./pages/AuthorBooksPage.tsx";
import { AuthorsPage } from "./pages/AuthorsPage.tsx";
import { BooksPage } from "./pages/BooksPage.tsx";
import { LibraryQueriesPage } from "./pages/LibraryQueriesPage.tsx";

export function App() {
  const path = window.location.pathname;

  return (
    <div>
      <nav>
        <a href="/authors">Authors</a> | <a href="/books">Books</a> | <a href="/author-books">Links</a> |{" "}
        <a href="/library-queries">Nested reads</a>
      </nav>

      {path === "/books" ? (
        <BooksPage />
      ) : path === "/author-books" ? (
        <AuthorBooksPage />
      ) : path === "/library-queries" ? (
        <LibraryQueriesPage />
      ) : (
        <AuthorsPage />
      )}
    </div>
  );
}
