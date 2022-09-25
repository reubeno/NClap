FROM mcr.microsoft.com/vscode/devcontainers/dotnet:6.0-focal

# Make sure pipe failures trigger build failures
SHELL ["/bin/bash", "-o", "pipefail", "-c"]

# Install native components
RUN apt-get update && \
    apt-get install -y --no-install-recommends \
        curl \
        dos2unix \
        git \
        gnupg2 \
        sudo \
        zsh \
        zsh-syntax-highlighting \
        zsh-autosuggestions \
    && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

# Switch to non-root user
USER vscode

# Select oh my zsh plugins
# Install 'bat' from GitHub release drop
# Install 'exa' from GitHub release drop
WORKDIR /tmp/work
RUN sed -i "s/^plugins=.*/plugins=(git zsh-autosuggestions zsh-syntax-highlighting)/" ~/.zshrc && \
    \
    curl https://github.com/sharkdp/bat/releases/download/v0.22.1/bat-musl_0.22.1_amd64.deb --output ./bat.deb && \
    sudo dpkg -i ./bat.deb && \
    sudo apt-get install -f && \
    \
    curl https://github.com/ogham/exa/releases/download/v0.10.1/exa-linux-x86_64-v0.10.1.zip --output ./exa.zip && \
    unzip ./exa.zip && \
    sudo mv ./bin/* /usr/bin && \
    sudo mv ./completions/exa.zsh /usr/share/zsh/vendor-completions/_exa && \
    \
    curl https://get.trunk.io -fsSL | sudo bash -s - -y && \
    sudo chmod a+r /usr/local/bin/trunk