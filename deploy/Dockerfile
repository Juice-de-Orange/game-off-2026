# The live version (P49, docs/DEPLOY.md): static WebGL build in an unprivileged nginx.
# Build context = this folder with the Unity build in ./site
FROM nginxinc/nginx-unprivileged:1.29.8-alpine
ARG GIT_SHA=unknown
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY site/ /usr/share/nginx/html/
USER root
RUN echo "$GIT_SHA" > /usr/share/nginx/html/version.txt && chown -R root:root /usr/share/nginx/html && chmod -R a+rX /usr/share/nginx/html
USER 101
