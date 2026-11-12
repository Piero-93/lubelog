# LubeLogger with sub-path support

This is a fork of [LubeLogger](https://github.com/hargata/lubelog) that can be served under a
sub-path behind a reverse proxy, e.g. `https://home.example.com/lubelogger/`. Upstream only
supports the root of a domain ([#775](https://github.com/hargata/lubelog/issues/775)).

Everything else is upstream LubeLogger: the image is rebuilt from each upstream release with a
small patch on top, on the `base-path` branch.

## Usage

```yaml
services:
  lubelogger:
    image: ghcr.io/piero-93/lubelogger:latest
    environment:
      - LUBELOGGER_BASE_PATH=/lubelogger
    volumes:
      - ./data:/App/data
      - ./keys:/root/.aspnet/DataProtection-Keys
```

The proxy must forward the full path, prefix included. With Caddy:

```
handle /lubelogger* {
	reverse_proxy lubelogger:8080
}
```

Use `handle`, not `handle_path`: the prefix must reach LubeLogger. Without
`LUBELOGGER_BASE_PATH`, or for requests without the prefix, it behaves like upstream.

If you use OpenID Connect, the redirect URL includes the prefix:
`https://home.example.com/lubelogger/Login/RemoteAuth`. For e-mail links, set
`LUBELOGGER_DOMAIN` to `https://home.example.com/lubelogger`.

## How it works

- `UsePathBase` on the server, plus a middleware that adds the prefix to redirects.
- A tag helper adds the prefix to `src`/`href` attributes, including stored paths (vehicle
  images, attachments, logos).
- In the browser, a jQuery `ajaxPrefilter` covers every request; navigations go through
  `withBasePath()`. `.github/base-path/check.sh` fails the build if new upstream code navigates
  without it.

## Updates

A daily workflow builds the newest upstream release with the patch, runs a smoke test under
`/lubelogger` and publishes `ghcr.io/piero-93/lubelogger:<version>` and `:latest`
(amd64 and arm64). If the patch stops applying, the image stays on the previous release until
the branch is rebased.

Issues about the sub-path go here; everything else goes upstream.
